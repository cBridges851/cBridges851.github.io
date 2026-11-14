# Spec: Move UI Strings into .resx Resource Files

## 1. Overview

All human-facing UI strings in the site are currently hardcoded inside logic and markup files (`.cs` location classes, `.razor` components, `App.razor`). This spec describes moving every one of those strings into a small set of `.resx` resource files — one per room plus one shared file for everything else — and consuming them through `IStringLocalizer`, so that copy changes can be made in one place instead of hunting through the codebase.

- **Primary goal:** centralise UI copy; no behaviour or visual change.
- **Secondary goal:** lay the groundwork for future localisation (additional `.resx` cultures) without further code changes.

## 2. Scope

### In scope
- Extracting every user-facing string from `MyCorner/Locations/*.cs`, `MyCorner/Commands/Command.cs`, `MyCorner/Pages/ConsoleComponent.razor`, and `MyCorner/App.razor` into `.resx`.
- Wiring `Microsoft.Extensions.Localization` into the Blazor WASM host and components.
- Updating `Location` subclasses and the console component to read copy from `IStringLocalizer<T>`.
- Preserving exact rendered output (HTML markup, interpolation, newlines).

### Out of scope
- `wwwroot/index.html` static strings ("An unhandled error has occurred.", "Reload", `<title>`) and `wwwroot/manifest.webmanifest` (`name`/`short_name`) — these are static assets rendered before Blazor boots; localising them needs a JS-based i18n step and is a separate effort.
- Adding non-English cultures or a culture picker.
- Changing copy wording, layout, or styling.
- The latent `ExecuteAction` catch bug (creates a `CommandResult.Failed` but does not return it) — noted in §10 as an optional, low-risk fix to bundle in.

## 3. Resource files

Three resource files, split by ownership:

| File | Owns | Marker type |
|---|---|---|
| `Resources/TardisConsoleRoom.resx` | Copy belonging to the TARDIS Console Room | `TardisConsoleRoom` |
| `Resources/Workshop.resx` | Copy belonging to the Project Workshop ("Projects" room) | `Workshop` |
| `Resources/Common.resx` | Everything not tied to a specific room: console chrome, app-level, location-base/shared messages, navigation prompts | `Common` |

Each file has a matching empty marker class in `MyCorner/Resources/`. A single `IStringLocalizer<T>` is injected per file. Keys keep a dotted area prefix (§4) so they stay unique and self-describing even if files are ever merged.

## 4. String inventory

Values are stored verbatim (including HTML) unless noted. `{0}`, `{1}` are composite-format placeholders.

### 4.1 `Common.resx` (not tied to a specific room)

| Proposed key | Current value (verbatim) | Source | Notes |
|---|---|---|---|
| `Console.HeaderTitle` | `cBridges.github.io, A Personal Website by Christa Bridges` | ConsoleComponent.razor | `<h2>` |
| `Console.Version` | `v2.0.0` | ConsoleComponent.razor | `<h2>` |
| `Console.Intro` | Multi-paragraph TARDIS/Christa intro ending with the LinkedIn link | ConsoleComponent.razor | Contains HTML (`<br>`, `<a>`). Rendered as `MarkupString`. |
| `Console.InputTitle` | `Command Input` | ConsoleComponent.razor | `title` attribute |
| `Console.InputPlaceholder` | `Enter command...` | ConsoleComponent.razor | `placeholder` |
| `Console.SubmitButton` | `GO` | ConsoleComponent.razor | submit `value` |
| `App.NotFoundTitle` | `Not found` | App.razor | `PageTitle` |
| `App.NotFoundMessage` | `Sorry, there's nothing at this address.` | App.razor | NotFound view |
| `General.RickrollResponse` | `Of course! I'd never let you down...` | Location.cs | `rickroll` command |
| `General.CommandError` | `An error occurred while executing the command. Please try again.` | Location.cs | exception path |
| `General.UnknownCommand` | `Sorry, I'm not sure what you mean. Try 'help' for available commands.` | Location.cs | fallback |
| `General.HelpTemplate` | `Available commands in {0}:<br>\n- help: Show this help text<br>\n{1}` | Location.cs | `{0}` = location name, `{1}` = location-specific help |
| `General.GoWherePrompt` | `Go where? Try 'help' for available destinations.` | both locations | shared navigation prompt |
| `General.CantGoThere` | `You can't go there from here. Try 'help' to see where you can go.` | both locations | shared navigation error |

### 4.2 `TardisConsoleRoom.resx`

| Proposed key | Current value (verbatim) | Source | Notes |
|---|---|---|---|
| `TardisConsoleRoom.Name` | `TARDIS Console Room` | TARDISConsoleRoom.cs | `Location.Name` |
| `TardisConsoleRoom.Description` | Full console-room description (multi-line) | TARDISConsoleRoom.cs | Contains `<br>` |
| `TardisConsoleRoom.GotoWorkshop` | `You walk through the door marked 'Projects'...` | TARDISConsoleRoom.cs | navigation success |
| `TardisConsoleRoom.Help` | `Location-specific commands:<br>\n- goto workshop: Visit the project workshop<br>` | TARDISConsoleRoom.cs | `GetLocationSpecificHelp` |

### 4.3 `Workshop.resx` (Projects room)

| Proposed key | Current value (verbatim) | Source | Notes |
|---|---|---|---|
| `Workshop.Name` | `Project Workshop` | Workshop.cs | `Location.Name` |
| `Workshop.Description` | Workshop intro + `{0}` project list + exits | Workshop.cs | `{0}` = `ListKeyProjects()` output |
| `Workshop.KeyProjectsHeader` | `Key projects:` | Workshop.cs | part of `ListKeyProjects` |
| `Workshop.ViewProjectHint` | `Use 'view [project-name]' to learn more about each project.` | Workshop.cs | |
| `Workshop.GitHubLink` | `GitHub with all projects: <a href="https://github.com/cBridges851/" target="_blank" rel="noopener noreferrer">https://github.com/cBridges851/</a>` | Workshop.cs | |
| `Workshop.Project.PersonalWebsite` | The "personal website" project description (multi-line HTML) | Workshop.cs | keyed by project |
| `Workshop.ViewWhat` | `View what? Try 'list' to see available projects.` | Workshop.cs | |
| `Workshop.ProjectNotFound` | `Project not found. Use 'list' to see available projects.` | Workshop.cs | |
| `Workshop.GotoConsole` | `You walk back to the TARDIS Console Room...` | Workshop.cs | |
| `Workshop.Help` | `Location-specific commands:<br>\n- list: Shows all projects<br>\n- view [project-name]: Get details about a specific project<br>\n- goto console: Return to the TARDIS Console Room<br>` | Workshop.cs | |

Notes on project names: the dictionary key `"personal website"` is both a command argument and a displayed label. It stays in code as an identifier (command input); only its description moves to resx. If project display names later need translating, add a `Workshop.Project.{Key}.Name` entry per project.

## 5. Proposed design

### 5.1 Resource layout

```
MyCorner/
  Resources/
    Common.cs                  # empty marker class (namespace MyCorner.Resources)
    Common.resx                # default (en) copy for shared/non-room strings
    TardisConsoleRoom.cs       # empty marker class
    TardisConsoleRoom.resx     # default (en) copy for the TARDIS Console Room
    Workshop.cs                # empty marker class
    Workshop.resx              # default (en) copy for the Project Workshop
    # <Name>.<culture>.resx added later for other languages
```

Strings are split by room so editing a room's copy only touches that room's file, while genuinely shared copy (navigation prompts, base location messages, console chrome, app-level) lives in `Common.resx`.

### 5.2 Marker classes

```csharp
namespace MyCorner.Resources;

public class Common { }
public class TardisConsoleRoom { }
public class Workshop { }
```

No generated designer file is required; `IStringLocalizer<T>` resolves resources by the type's full name.

### 5.3 Consumption pattern

- **Components (.razor):** inject the localizer(s) needed, e.g. `@inject IStringLocalizer<Common> L` for chrome, then `@L["Key"]` for plain text. For values containing HTML, cast: `@((MarkupString)L["Key"].Value)`.
- **C# classes:** constructor receives the localizer(s) it needs; store as `protected readonly IStringLocalizer<...>`. Read via `Strings["Key"].Value` or `Strings["Key", arg0, arg1].Value` for composite formatting.
- Interpolation uses the localizer's `params object[]` indexer, which applies `string.Format` with the current culture.

Which localizer where:
- `Location` base class holds `IStringLocalizerFactory StringLocalizerFactory` and a resolved `IStringLocalizer Common` (for `General.UnknownCommand`, `General.CommandError`, `General.HelpTemplate`, `General.RickrollResponse`, and the shared `General.GoWherePrompt`/`General.CantGoThere`).
- `TARDISConsoleRoom` additionally resolves `IStringLocalizer Room` via the factory; `Workshop` does the same.
- `ConsoleComponent.razor` injects `IStringLocalizer<Common>` (chrome/intro all live in Common).
- `App.razor` injects `IStringLocalizer<Common>`.

### 5.4 Culture setup

`Program.cs` calls `builder.Services.AddLocalization();` and sets a default culture (`en-GB`) via `CultureInfo.DefaultThreadCurrentCulture` / `DefaultThreadCurrentUICulture`. No browser-language detection is added in this spec (default English), keeping the change minimal.

## 6. Architecture changes

### 6.1 `MyCorner/MyCorner.csproj`

Add the localization package (version aligned to the existing 8.0.x references):

```xml
<PackageReference Include="Microsoft.Extensions.Localization" Version="8.0.16" />
```

No other csproj changes are required; SDK-style projects embed `*.resx` as manifest resources automatically.

### 6.2 `MyCorner/Program.cs`

```csharp
using System.Globalization;
// ...
builder.Services.AddLocalization();
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");
```

### 6.3 `MyCorner/_Imports.razor`

Add:
```razor
@using Microsoft.Extensions.Localization
@using MyCorner.Resources
```

### 6.4 `MyCorner/Locations/Location.cs`

- Constructor signature becomes `Location(IJSRuntime js, IStringLocalizerFactory stringLocalizerFactory)`.
- Store `protected readonly IStringLocalizerFactory StringLocalizerFactory` and `protected readonly IStringLocalizer Common = stringLocalizerFactory.Create(typeof(MyCorner.Resources.Common));`.
- Replace hardcoded strings with `Common[...].Value` (composite form for `HelpTemplate`).
- `GetHelpText()` uses `Common["General.HelpTemplate", Name, GetLocationSpecificHelp()]`.
- Optional: fix the catch block to `return CommandResult.Failed(Common["General.CommandError"].Value);`.

### 6.5 `MyCorner/Locations/TARDISConsoleRoom.cs` and `Workshop.cs`

- Constructor signature becomes `(IJSRuntime js, IStringLocalizerFactory stringLocalizerFactory)`, passing the factory to `base` and resolving `Room` via `stringLocalizerFactory.Create(typeof(MyCorner.Resources.TardisConsoleRoom/Workshop))`.
- `Name`/`Description` assigned from `Room[...].Value` (interpolated templates use the composite indexer).
- Room-specific handlers and `GetLocationSpecificHelp()` read from `Room`; shared navigation prompts read from `Common` (inherited).
- Sibling rooms are constructed with `new TARDISConsoleRoom(js, StringLocalizerFactory)` / `new Workshop(js, StringLocalizerFactory)`. (Resolved: option A from §8.)

### 6.6 `MyCorner/Pages/ConsoleComponent.razor`

- Add `@inject IStringLocalizerFactory StringLocalizerFactory` and resolve `var L = StringLocalizerFactory.Create(typeof(MyCorner.Resources.Common));`.
- Replace the header/version/intro markup with `@((MarkupString)L["Console.HeaderTitle"].Value)` etc.; the intro is a single resx value rendered as `MarkupString`.
- `title`/`placeholder`/`value` attributes use `@L["..."].Value` (plain text).
- `new TARDISConsoleRoom(js, StringLocalizerFactory)` updated to the new constructor.

### 6.7 `MyCorner/App.razor`

- Add `@inject IStringLocalizer<Common> L`.
- `PageTitle` and NotFound message read from `L["App.NotFoundTitle"]` / `L["App.NotFoundMessage"]`.

## 7. Migration steps

1. Add package + `Program.cs` registration (§6.1–6.2).
2. Add `_Imports.razor` usings (§6.3).
3. Create `Resources/{Common,TardisConsoleRoom,Workshop}.cs` marker classes and matching `.resx` files.
4. Populate each `.resx` with the keys from §4, preserving HTML and using `{0}`, `{1}` placeholders for interpolated strings.
5. Update `Location.cs` (§6.4), then `TARDISConsoleRoom.cs` and `Workshop.cs` (§6.5).
6. Update `ConsoleComponent.razor` (§6.6) and `App.razor` (§6.7).
7. Build and run; verify rendered output is byte-for-byte equivalent to before (compare against a saved baseline of the page).
8. Run Lighthouse CI to confirm thresholds.

## 8. Resolved: how locations obtain a sibling room's localizer

`Workshop` constructs `new TARDISConsoleRoom(js, ...)` at runtime, and `TARDISConsoleRoom` constructs `new Workshop(js, ...)` (currently Workshop doesn't, but TARDIS could in future). Passing concrete `IStringLocalizer<T>` instances through constructors means a room must resolve another room's localizer. Options:

- **A.** Inject `IStringLocalizerFactory` into `Location` and resolve any `IStringLocalizer<T>` on demand via `Factory.Create(typeof(T))`. Cleanest for cross-room construction; rooms call `Common = Factory.Create(typeof(Common))` etc. in their constructor.
- **B.** Keep concrete localizers in constructors and pass them along when constructing sibling rooms (rooms store and forward their own `Common`/`Room` instances).

Recommendation: **A**, because it avoids threading multiple localizers through every constructor and makes adding rooms trivial. Finalise before implementation; the rest of the spec is written against concrete localizers for clarity and is unaffected by this choice.

## 9. Markup / HTML handling

- resx values may contain HTML; they are stored as plain text and rendered via `MarkupString` in Blazor (unchanged from current behaviour).
- When authoring `.resx` by hand, XML-reserved characters in values must be escaped (`<` → `&lt;`, `&` → `&amp;`; `"` is fine inside element text). Prefer the Visual Studio/IDE resource editor, which escapes automatically. Mis-escaped values fail the build or render literally.
- Multi-line values: use `xml:space="preserve"` on the `<value>` element (the editor adds this) so leading/trailing whitespace and newlines are retained.

## 10. Acceptance criteria

- [ ] `MyCorner/Resources/{Common,TardisConsoleRoom,Workshop}.resx` exist and contain the keys listed in §4.
- [ ] No user-facing string literal remains in `MyCorner/Locations/*.cs`, `MyCorner/Commands/*.cs`, `MyCorner/Pages/ConsoleComponent.razor`, or `MyCorner/App.razor` (identifiers, command verbs, URLs, and CSS class names are allowed).
- [ ] `dotnet build MyCorner/MyCorner.csproj` succeeds.
- [ ] `dotnet publish MyCorner -c Release` succeeds.
- [ ] Rendered page output is visually and textually identical to the pre-change version.
- [ ] Lighthouse CI accessibility ≥ 0.9 and best-practices ≥ 0.9.
- [ ] Adding a new culture requires only new `*.resx` files (no code changes).
- [ ] Adding a new room requires only a new marker class + `.resx` plus its location subclass.

## 11. Risks

- **HTML encoding in Razor:** forgetting the `MarkupString` cast on a markup-bearing resx value will render tags literally. Mitigate with code review and the inventory table (§4 marks HTML-bearing keys).
- **resx XML escaping:** hand-edited markup values must be escaped (§9).
- **Performance:** localisation adds a small satellite-assembly download; well within the Lighthouse performance threshold (≥ 0.2, warn).
- **Project names as identifiers:** kept in code; only descriptions are localised (§4.3 note). Revisit if project display names must translate.
- **`Common.resx` growth:** all non-room copy lands here. If it becomes unwieldy, split into further files (e.g. `ConsoleComponent.resx`, `App.resx`) using the same pattern — no architectural change needed.

## 12. Optional bundle: fix latent catch bug

In `Location.ExecuteAction`, the catch block currently calls `CommandResult.Failed(...)` without returning it, so exceptions fall through to the unknown-command message. While editing this method for localisation, change it to `return CommandResult.Failed(Common["General.CommandError"].Value);`. Low risk, improves error reporting. Out of scope if you want the spec kept strictly to copy extraction.