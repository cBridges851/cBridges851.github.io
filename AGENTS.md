# AGENTS.md

## Project

- Single .NET 8 Blazor WebAssembly app in `MyCorner/` (`MyCorner.csproj`, solution `MyCorner.sln`).
- GitHub Pages user site (`<base href="/" />`); deployed from `MyCorner/bin/Release/net8.0/publish/wwwroot`.

## Commands

- Build: `dotnet build MyCorner/MyCorner.csproj` (or `dotnet build MyCorner.sln`)
- Run dev server: `dotnet run --project MyCorner` (http://localhost:5219)
- Publish (matches CI): `dotnet publish MyCorner -c Release`
- Lighthouse CI (after publish): `npx @lhci/cli autorun`
- There is no test project; verification is build + Lighthouse CI.

## Structure

- `MyCorner/Program.cs`: registers `App` into `#app` and `HeadOutlet`.
- `MyCorner/App.razor`: router with `MainLayout`; only page is `MyCorner/Pages/Home.razor` (`/`), which hosts `MyCorner/Pages/ConsoleComponent.razor`.
- `MyCorner/Pages/ConsoleComponent.razor`: game loop splits input into verb + args and calls `CurrentLocation.ExecuteAction`.
- `MyCorner/Locations/Location.cs`: abstract base storing command handlers; subclasses implement `RegisterLocationCommands()` and `GetLocationSpecificHelp()`.
- `MyCorner/Locations/TARDISConsoleRoom.cs` and `Workshop.cs`: the two locations; navigation is wired manually in both directions.
- `MyCorner/Commands/Command.cs`: `Command` record and `CommandResult`.

## Conventions / gotchas

- Nullable reference types and implicit usings are enabled.
- To add a location: subclass `Location`, set `Name`/`Description`, call `InitialiseCommands()` in the constructor, and add matching `goto`/`go` handlers in both the new and existing locations.
- `Location.ExecuteAction`'s `catch` creates a `CommandResult.Failed(...)` but does not return it, so exceptions surface as the generic unknown-command message.
- `.nojekyll` must remain so GitHub Pages serves `_framework` assets.
- `.gitattributes` marks `*.js`, `*.json`, `*.css`, `*.html` as binary; `git diff` may hide textual changes. Use `git diff --text`.
- CI (`.github/workflows/deploy.yml`) deploys only on push to `master`.
- Lighthouse CI thresholds: accessibility/best-practices >= 0.9 (error), seo >= 0.9 (warn), performance >= 0.2 (warn); collects `/` and `/projects`.
- Dev service worker (`wwwroot/service-worker.js`) is a no-op fetch handler; the published variant is configured via the csproj `ServiceWorker` item.

## Definition of Done
- Build passes
- New and existing tests pass
- Lighthouse CI checks pass