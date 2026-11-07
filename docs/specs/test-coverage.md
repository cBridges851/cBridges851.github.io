# Spec: Retrospectively Add Test Coverage

## 1. Overview

The site (`MyCorner`) currently has no automated tests. This spec defines the work required to add end-to-end (E2E) test coverage for the user journeys described in the product expectations below, executed from a fresh worktree/branch based on `master`.

The scenarios are expressed as Gherkin-style acceptance criteria and map to observable UI behaviour of the Blazor WebAssembly app: page load rendering, command input handling, location navigation, JS-interopt `window.open` calls, and rendered links.

- **App under test:** `MyCorner` (Blazor WebAssembly, .NET 8), served at the dev URL `http://localhost:5219` or a published static host.
- **Test layer:** End-to-end browser tests (recommended: Playwright Test, TypeScript) exercising a real browser, because the scenarios involve DOM rendering, form submission, navigation state, and `window.open` popups.
- **Out of scope for this spec:** Unit-level bUnit component tests and CI pipeline wiring are mentioned as follow-ups; the primary deliverable is the E2E suite covering the listed journeys.

## 2. Scope

### In scope

- Creating a test project/suite that automates every scenario in section 4.
- Adding stable selectors (or documenting accessible selectors) so tests are robust.
- A documented run command and (optionally) CI integration to execute the suite.
- Updating `AGENTS.md`/Definition of Done references if tests become a gate.

### Out of scope

- Rewriting application logic or fixing the known `Location.ExecuteAction` catch-without-return defect (noted in `AGENTS.md`); tests assert current behaviour only.
- Visual regression / screenshot diffing.
- Performance or accessibility audits (already covered by Lighthouse CI).
- The `/projects` route (not implemented).
- Adding new game content or locations.

## 3. Current state inventory

| Area | Current state |
|---|---|
| Test projects | None. `MyCorner.sln` contains only `MyCorner`. |
| Test runner | None configured. Verification today is `dotnet build` + Lighthouse CI. |
| Selectors available | `input[name="command-input"]`, `input[type="submit"]` (value `GO`), `header h1`, `.console-component__output`, anchor text `LinkedIn`, `title="Command Input"`, `placeholder="Enter command..."`. |
| JS interop | `window.openInNewTab(url)` in `wwwroot/scripts/openInNewTab.js`, invoked for `rickroll` and implicitly via anchor `target="_blank"` for LinkedIn. |
| State | `ConsoleComponent` holds `CurrentLocation`; navigation replaces it and appends name/description to `ConsoleOutput`. |

## 4. Acceptance scenarios (Gherkin)

The following scenarios define the expected behaviour. Each `Given the user has loaded <host name>` means the test navigates to the root URL of a running instance and waits for the app to render.

### S1 - Initial page load

```gherkin
Given the user has loaded <host name>
When the page is loaded
Then the header at the top indicates that the user is located in the "TARDIS Console Room"
Then there is a blurb about the user being in the TARDIS
Then the available exits are outputted
```

### S2 - LinkedIn link

```gherkin
Given the user has loaded <host name>
When they click on the "LinkedIn" link
Then a new window opens to Christa Bridges' LinkedIn profile
```

### S3 - help command

```gherkin
Given the user has loaded <host name>
When they enter "help" into the command text box and press "GO"
Then the general commands are outputted
Then the location specific commands are outputted
```

### S4 - Unknown command

```gherkin
Given the user has loaded <host name>
When they enter random gibberish into the command text box and press "GO"
Then the output indicates that it does not understand what the user means
Then the output indicates that they can enter "help" to see available commands
```

### S5 - rickroll command

```gherkin
Given the user has loaded <host name>
When they type "rickroll" into the command text box and press "GO"
Then a new window opens to "Never Gonna Give You Up" on YouTube
```

### S6 - Navigate to Project Workshop

```gherkin
Given the user has loaded <host name>
When they enter "go west" into the command text box and press "GO"
Then the header at the top now indicates that they are in the Project Workshop
Then the projects are listed in the output
```

### S7 - Return to TARDIS Console Room

```gherkin
Given the user has navigated to the Project Workshop
When they enter "go east" into the command text box and press "GO"
Then the header at the top indicates that they are back in the TARDIS Console Room
```

### S8 - View a project

```gherkin
Given the user has navigated to the Project Workshop
When they enter "view personal website" and press "GO"
Then a blurb about the project is outputted
Then there is a link to https://github.com/cBridges851/cBridges851.github.io/
```

### S9 - Empty command

```gherkin
Given the user has loaded <host name>
When they press "GO" without entering a command
Then the output indicates that it does not understand what the user means
```

### S10 - "go" with no destination

```gherkin
Given the user has loaded <host name>
When they enter "go" into the command text box and press "GO"
Then the output asks where the user wants to go
```

### S11 - "view" with no project

```gherkin
Given the user has navigated to the Project Workshop
When they enter "view" into the command text box and press "GO"
Then the output asks what the user wants to view
```

## 5. Proposed implementation

### 5.1 Test framework and location

- Add a `tests/e2e` directory at the repository root containing a Playwright Test project (TypeScript). TypeScript Playwright is the chosen stack (simpler popup handling and HTML reporting).

Proposed files (Playwright Test variant):

```
tests/
  e2e/
    package.json
    playwright.config.ts
    specs/
      console.spec.ts
```

### 5.2 Selectors and page object

Add `data-testid` attributes to the markup (the one markup change proposed) and select via them:

- `header` element: `data-testid="location-header"`
- output container: `data-testid="console-output"`
- command input: `data-testid="command-input"`
- submit button: `data-testid="submit-button"`
- LinkedIn anchor: `data-testid="linkedin-link"`

A small page object (`ConsolePage`) should wrap:

- `locationHeader` = `page.getByTestId('location-header')`
- `commandInput` = `page.getByTestId('command-input')`
- `goButton` = `page.getByTestId('submit-button')`
- `output` = `page.getByTestId('console-output')`
- `linkedInLink` = `page.getByTestId('linkedin-link')`

### 5.3 Test harness / running the app

Option A (dev server, recommended for local dev):

```bash
# terminal 1
dotnet run --project MyCorner
# terminal 2
cd tests/e2e && npx playwright test
```

Playwright `webServer` config can launch `dotnet run --project MyCorner` automatically:

```ts
webServer: {
  command: 'dotnet run --project ../../MyCorner',
  url: 'http://localhost:5219',
  reuseExistingServer: !process.env.CI,
  timeout: 120000,
},
use: { baseURL: 'http://localhost:5219' },
```

Option B (published static host, closer to production):

```bash
dotnet publish MyCorner -c Release
npx serve -s MyCorner/bin/Release/net8.0/publish/wwwroot -l 8080
```

and point `baseURL` at `http://localhost:8080`.

### 5.4 Scenario implementation notes

- **Browser projects:** Chromium only.

- **Popups (S2, S5):** Avoid loading the remote sites. Override `window.open` / `window.openInNewTab` via an init script so the URL passed is captured without navigation:

  ```ts
  await page.addInitScript(() => {
    window.__openedUrls = [];
    window.open = (url: string) => { window.__openedUrls.push(String(url)); return null; };
    window.openInNewTab = (url: string) => { window.__openedUrls.push(String(url)); };
  });
  ```

  Then assert `window.__openedUrls` contains the expected URL. For the LinkedIn anchor, verify `target="_blank"` and the captured URL; no popup event assertion is required. Assert URLs `linkedin.com/in/christa-bridges` and `youtube.com/watch?v=dQw4w9WgXcQ`.

- **Command submission:** Fill `commandInput`, then click `goButton` (or press Enter). Wait for the echoed `> help` line and the resulting output to appear before asserting, e.g. `await expect(output).toContainText('...')`.

- **Stateful navigation (S7, S8):** These start "in the Project Workshop". Implement a helper `gotoWorkshop(page)` that performs S6's steps (or navigates via `go west`) so each test is independent and does not rely on execution order.

- **Assertions per scenario:**

  | Scenario | Key assertions |
  |---|---|
  | S1 | `locationHeader` has text `TARDIS Console Room`; output contains `TARDIS`/`bigger on the inside`; output contains `Available exits` and `workshop (west)`. |
  | S2 | `window.__openedUrls` contains `linkedin.com/in/christa-bridges`; anchor has `target="_blank"`. |
  | S3 | Output contains `help: Show this help text` (general) and `Location-specific commands` + `goto workshop` (location-specific). |
  | S4 | Output contains `Sorry, I'm not sure what you mean` and `Try 'help'`. |
  | S5 | `window.__openedUrls` contains `youtube.com/watch?v=dQw4w9WgXcQ`; output contains `Of course! I'd never let you down`. |
  | S6 | `locationHeader` text `Project Workshop`; output contains `Key projects` and `personal website`. |
  | S7 | `locationHeader` text `TARDIS Console Room`. |
  | S8 | Output contains `The site you're looking at now` and a link with href `https://github.com/cBridges851/cBridges851.github.io/`. |
  | S9 | Output contains `Sorry, I'm not sure what you mean` (empty input). |
  | S10 | Output contains `Go where?` (or `Try 'help'`). |
  | S11 | Output contains `View what?` (or `Try 'list'`). |

### 5.5 Example test (illustrative)

```ts
// tests/e2e/specs/console.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Console', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.__openedUrls = [];
      window.open = (url: string) => { window.__openedUrls.push(String(url)); return null; };
      window.openInNewTab = (url: string) => { window.__openedUrls.push(String(url)); };
    });
    await page.goto('/');
    await expect(page.getByTestId('location-header')).toBeVisible({ timeout: 60000 });
    await expect(page.getByTestId('location-header')).toHaveText('TARDIS Console Room');
  });

  test('S1 initial page load', async ({ page }) => {
    const header = page.getByTestId('location-header');
    await expect(header).toHaveText('TARDIS Console Room');
    await expect(page.getByTestId('console-output')).toContainText('TARDIS');
    await expect(page.getByTestId('console-output')).toContainText('Available exits');
    await expect(page.getByTestId('console-output')).toContainText('workshop (west)');
  });

  test('S2 LinkedIn opens new window', async ({ page }) => {
    await expect(page.getByTestId('linkedin-link')).toHaveAttribute('target', '_blank');
    const popupPromise = page.waitForEvent('popup');
    await page.getByTestId('linkedin-link').click();
    const popup = await popupPromise;
    await expect.poll(() => page.evaluate(() => (window as any).__openedUrls ?? [])).toContain('https://www.linkedin.com/in/christa-bridges/');
    await popup.close();
  });

  test('S3 help command', async ({ page }) => {
    const input = page.getByTestId('command-input');
    await input.fill('help');
    await page.getByTestId('submit-button').click();
    const output = page.getByTestId('console-output');
    await expect(output).toContainText('help: Show this help text');
    await expect(output).toContainText('Location-specific commands');
    await expect(output).toContainText('goto workshop');
  });

  test('S4 unknown command', async ({ page }) => {
    const input = page.getByTestId('command-input');
    await input.fill('asdfghjkl');
    await page.getByTestId('submit-button').click();
    const output = page.getByTestId('console-output');
    await expect(output).toContainText("Sorry, I'm not sure what you mean");
    await expect(output).toContainText("Try 'help'");
  });

  test('S5 rickroll opens YouTube', async ({ page }) => {
    const input = page.getByTestId('command-input');
    await input.fill('rickroll');
    const popupPromise = page.waitForEvent('popup');
    await page.getByTestId('submit-button').click();
    const popup = await popupPromise;
    await expect(popup).toHaveURL(/youtube\.com\/watch\?v=dQw4w9WgXcQ/);
  });

  test('S6 go west -> workshop', async ({ page }) => {
    const input = page.getByTestId('command-input');
    await input.fill('go west');
    await page.getByTestId('submit-button').click();
    await expect(page.getByTestId('location-header')).toHaveText('Project Workshop');
    const output = page.getByTestId('console-output');
    await expect(output).toContainText('Key projects');
    await expect(output).toContainText('personal website');
  });

  test('S7 go east -> console room', async ({ page }) => {
    // arrange: reach workshop
    const input = page.getByTestId('command-input');
    await input.fill('go west');
    await page.getByTestId('submit-button').click();
    await expect(page.getByTestId('location-header')).toHaveText('Project Workshop');
    // act
    await page.getByTestId('command-input').fill('go east');
    await page.getByTestId('submit-button').click();
    await expect(page.getByTestId('location-header')).toHaveText('TARDIS Console Room');
  });

  test('S8 view personal website', async ({ page }) => {
    const input = page.getByTestId('command-input');
    await input.fill('go west');
    await page.getByTestId('submit-button').click();
    await page.getByTestId('command-input').fill('view personal website');
    await page.getByTestId('submit-button').click();
    const output = page.getByTestId('console-output');
    await expect(output).toContainText('The site you're looking at now');
    await expect(output.locator('a[href="https://github.com/cBridges851/cBridges851.github.io/"]')).toBeVisible();
  });
});
```

### 5.6 CI integration (out of scope)

CI execution is out of scope for this spec; the suite is intended to run locally against the dev server. (A workflow can be added later without changing the test code.)

### 5.7 Additional testing considerations (not covered above)

- **First-load latency.** Blazor WASM downloads the runtime on first navigation. Rather than fixed 60–120s sleeps, wait for a concrete render signal, e.g. `await expect(page.getByTestId('location-header')).toBeVisible({ timeout: 60000 })`, and set `webServer.timeout` to cover server startup. Consider `workers: 1` to avoid overloading the dev server.
- **Blazor render timing.** Output is appended after `await ExecuteAction`. Rely on Playwright auto-waiting (`expect(...).toContainText(...)`) rather than fixed sleeps; never assert immediately after clicking GO.
- **Parallelism and isolation.** Playwright creates a fresh context per test by default, which gives isolation, but parallel workers hitting a single dev server can contend. Set `fullyParallel: false` (or `workers: 1`) for the E2E project.
- **Dev server only.** Tests run against `dotnet run --project MyCorner` (http://localhost:5219), avoiding published service-worker caching.
- **Console error checking.** Each test should assert that no uncaught browser console errors occur (via `page.on('console')` / `page.on('pageerror')`), failing the test if any error-level message is emitted.
- **Echoed command.** Tests should also assert the typed command is echoed in the output (e.g. `> help`) before asserting the result.

## 6. Files to change / add

| File | Change |
|---|---|
| `tests/e2e/package.json` | New. Playwright Test dependency and scripts. |
| `tests/e2e/playwright.config.ts` | New. `webServer`, `baseURL`, browser projects. |
| `tests/e2e/specs/console.spec.ts` | New. Scenarios S1-S11. |
| `MyCorner/Pages/ConsoleComponent.razor` | Required: add `data-testid` attributes for stable selectors. |
| `.github/workflows/e2e.yml` | Optional: CI execution. |
| `AGENTS.md` | Update Definition of Done to mention E2E tests. |

No changes to command logic, locations, or styling are required; the only markup change is the addition of `data-testid` attributes.

## 7. Risks / open questions

- **Blazor render timing:** Command output is appended after `ProcessInput` awaits `ExecuteAction`; tests should wait on `expect(...).toContainText(...)` with Playwright's auto-waiting rather than fixed sleeps.
- **`rickroll` depends on JS interop:** Requires the published/dev server to load `scripts/openInNewTab.js`; ensure the script tag remains in `index.html`.
- **State leakage:** Tests must not share state; each test navigates fresh (or uses an isolated browser context). Playwright provides a fresh context per test by default.

- **Selector drift:** If CSS classes or text change, tests may break; `data-testid` attributes mitigate this (section 5.2).
- **Host placeholder:** `<host name>` in scenarios maps to the running instance URL; tests use `baseURL` from config.
- **Browser installation in CI:** `npx playwright install --with-deps` adds time; cache the browser binaries.

## 8. Acceptance criteria

- [ ] A runnable TypeScript Playwright E2E suite exists under `tests/e2e` covering S1-S11.
- [ ] `npx playwright test` passes against a running instance.
- [ ] S2 and S5 verify a new browser tab/window opens to the expected URL.
- [ ] S6/S7 verify the top `h1` location name changes.
- [ ] S8 verifies both the project blurb and the GitHub link.
- [ ] Tests are independent (fresh navigation per test).
- [ ] `dotnet build MyCorner/MyCorner.csproj` still succeeds.
- [ ] Lighthouse CI thresholds remain met.
- [ ] (Optional) CI workflow runs the suite on push/PR.

## 9. Definition of Done (updated)

- Build passes.
- New and existing tests pass (E2E suite green).
- Lighthouse CI checks pass.