# Spec: Update Site Styling to Personal Brand Colour Scheme

## 1. Overview

Update the Blazor WebAssembly site's styling so that all accent/brand surfaces use Christa Bridges' new personal branding colour, while retaining the existing dark background (`#000000`) and light text (`#ffffff`).

- **Primary brand colour:** `#c000f5` (vivid purple)
- **Background:** `#000000` (unchanged)
- **Text:** `#ffffff` (unchanged)

## 2. Scope

### In scope

- Replacing the ad-hoc accent colours currently hardcoded across `wwwroot/css/app.css`, `Pages/ConsoleComponent.razor.css`, and `wwwroot/manifest.webmanifest` with a tokenised brand palette.
- Introducing CSS custom properties (design tokens) so future colour changes are single-point edits.
- Updating the PWA manifest `theme_color` and adding the mobile browser `theme-color` meta tag.
- Contrast/accessibility verification against the Lighthouse accessibility threshold (≥ 0.9).

### Out of scope

- Changes to layout, typography, fonts, or component structure.
- Light mode / theme switching.
- Replacing the favicon or other image assets.
- The `/projects` route (not implemented; unrelated to this spec).

## 3. Current colour inventory

| File | Selector / key | Current value | Role |
|---|---|---|---|
| `wwwroot/css/app.css` | `html, body` background | `#000000` | Page background (keep) |
| `wwwroot/css/app.css` | `html, body` color | `#ffffff` | Page text (keep) |
| `wwwroot/css/app.css` | `h2` color | `#B2364C` | Section title accent (replace) |
| `wwwroot/css/app.css` | `h3` color | `#72BA00` | Output header accent (replace) |
| `wwwroot/css/app.css` | `a` color | `#00BFFF` | Link colour (replace) |
| `wwwroot/css/app.css` | `input` color | `#ffffff` | Input text (keep) |
| `wwwroot/css/app.css` | `input[type=submit]` background | `#394669` | Button background (replace) |
| `wwwroot/css/app.css` | `input[type=submit]` border | `#2E3853` | Button border (replace) |
| `wwwroot/css/app.css` | `.loading-progress circle` stroke | `#e0e0e0` | Spinner track (keep) |
| `wwwroot/css/app.css` | `.loading-progress circle:last-child` stroke | `#1b6ec2` | Spinner accent (replace) |
| `wwwroot/css/app.css` | `.output-container` border | `black` | Container border (replace) |
| `wwwroot/css/app.css` | `#blazor-error-ui` background | `lightyellow` | Error banner (keep) |
| `Pages/ConsoleComponent.razor.css` | `header` background | `#ffffff` | Console header bar (keep) |
| `Pages/ConsoleComponent.razor.css` | `header` color | `#000000` | Console header text (keep) |
| `Pages/ConsoleComponent.razor.css` | `header h1` color | (inherit) | Console title text (new rule → brand) |
| `Pages/ConsoleComponent.razor.css` | `.console-component__input` background | `#3E3E3E` | Input bar surface (keep) |
| `wwwroot/manifest.webmanifest` | `background_color` | `#ffffff` | PWA splash (change to #000000) |
| `wwwroot/manifest.webmanifest` | `theme_color` | `#03173d` | PWA/browser chrome (replace) |

## 4. Proposed design tokens

Introduce a `:root` block at the top of `wwwroot/css/app.css`:

```css
:root {
    /* Brand */
    --color-brand: #c000f5;
    --color-brand-dark: #9a00c4;   /* brand mixed with black (~20%) — borders, hover/active */
    --color-brand-light: #d966ff;  /* brand mixed with white (~30%) — secondary accents */

    /* Neutrals (unchanged intent) */
    --color-bg: #000000;
    --color-text: #ffffff;
    --color-surface: #3e3e3e;
    --color-muted: #e0e0e0;        /* spinner track, placeholders */

    /* Status (unchanged) */
    --color-warning-bg: lightyellow;
}
```

Derived shades are final values (brand darkened/lightened by ~20%/~30%); they are not tunable during implementation.

## 5. Colour mapping (before → after)

### `wwwroot/css/app.css`

| Selector | Property | Before | After |
|---|---|---|---|
| `html, body` | `background-color` | `#000000` | `var(--color-bg)` |
| `html, body` | `color` | `#ffffff` | `var(--color-text)` |
| `h2` | `color` | `#B2364C` | `var(--color-brand)` |
| `h3` | `color` | `#72BA00` | `var(--color-brand-light)` |
| `a` | `color` | `#00BFFF` | `var(--color-brand)` |
| `input` | `color` | `#ffffff` | `var(--color-text)` |
| `input[type=submit]` | `background-color` | `#394669` | `var(--color-brand)` |
| `input[type=submit]` | `border` | `1px groove #2E3853` | `1px groove var(--color-brand-dark)` |
| `input[type=submit]` | `color` | (inherit) | `var(--color-text)` (explicit) |
| `input[type=submit]:hover` | `background-color` | (none) | `var(--color-brand-dark)` (new rule) |
| `input[type=submit]:focus` | `background-color` | (none) | `var(--color-brand-dark)` (new rule) |
| `input[type=submit]:active` | `background-color` | (none) | `var(--color-brand-dark)` (new rule) |
| `:focus-visible` (global) | `outline` | (browser default) | `2px solid var(--color-brand)` (new rule) |
| `.loading-progress circle` | `stroke` | `#e0e0e0` | `var(--color-muted)` |
| `.loading-progress circle:last-child` | `stroke` | `#1b6ec2` | `var(--color-brand)` |
| `.output-container` | `border` | `1px solid black` | `1px solid black` (unchanged, intentionally invisible) |

Additional enhancement (optional but recommended):
- Add `::selection { background: var(--color-brand-dark); color: var(--color-text); }` so text selection reflects the brand.

### `Pages/ConsoleComponent.razor.css`

| Selector | Property | Before | After |
|---|---|---|---|
| `header` | `background-color` | `#ffffff` | `#ffffff` (unchanged) |
| `header` | `color` | `#000000` | `#000000` (unchanged) |
| `header h1` | `color` | (inherit) | `#000000` (new rule, kept black) |
| `.console-component__input` | `background-color` | `#3E3E3E` | `var(--color-surface)` |
| `.console-component__input input[type=text]` | `background-color` | `transparent` | `transparent` (unchanged) |

Because `.razor.css` is scoped, the `:root` tokens must be available globally; they are defined in `app.css`, which is loaded in `index.html`, so scoped components inherit them via CSS cascade. No `:root` redefinition is needed in the scoped file.

### `wwwroot/manifest.webmanifest`

| Key | Before | After |
|---|---|---|
| `background_color` | `#ffffff` | `#000000` (match site) |
| `theme_color` | `#03173d` | `#c000f5` |

### `wwwroot/index.html` (in scope)

Add to `<head>`:
```html
<meta name="theme-color" content="#c000f5" />
```
so mobile browser chrome matches the brand.

## 6. Accessibility / contrast audit

Computed contrast ratios (WCAG 2.1):

| Foreground | Background | Ratio | Result |
|---|---|---|---|
| `#c000f5` (brand) | `#000000` (bg) | ≈ 4.56:1 | Pass AA (normal + large) |
| `#c000f5` (brand) | `#ffffff` | ≈ 4.61:1 | Pass AA |
| `#ffffff` (text) | `#c000f5` (brand, header) | ≈ 4.61:1 | Pass AA (large text; borderline for small) |
| `#d966ff` (brand-light) | `#000000` | ≈ 7.29:1 | Pass AA/AAA |
| `#9a00c4` (brand-dark) | `#000000` | ≈ 3.17:1 | Fail AA normal — use only for borders/decorative/large UI, never small body text |

Notes:
- The console header bar keeps its white background and black text, including the `<h1>` title; brand colour is not used in the header.
- `brand-dark` is restricted to borders, button borders, and hover/active states on large elements.
- Lighthouse accessibility must remain ≥ 0.9. Run Lighthouse CI after implementation; if contrast regressions appear, prefer `brand-light` for any text on dark backgrounds.

## 7. Files to change

1. `MyCorner/wwwroot/css/app.css` — add `:root` tokens; replace hardcoded values; add `::selection` (optional).
2. `MyCorner/Pages/ConsoleComponent.razor.css` — replace header/input values with tokens.
3. `MyCorner/wwwroot/manifest.webmanifest` — update `theme_color`.
4. `MyCorner/wwwroot/index.html` — add `theme-color` meta (optional).
5. `docs/specs/brand-colour-scheme.md` — this spec (already created).

No C# or Razor markup changes are required.

## 8. Implementation notes

- Keep the no-op dev service worker and published variant untouched.
- Do not alter `.nojekyll`.
- Tokens are global (`app.css`), so scoped `.razor.css` files reference them without redefinition.
- Preserve existing responsive rules and layout dimensions.
- Verify `dotnet build MyCorner/MyCorner.csproj` passes (CSS is not compiled, but scoped CSS bundling is).
- Verify `dotnet publish MyCorner -c Release` and Lighthouse CI (`npx @lhci/cli autorun`) pass thresholds.

## 9. Acceptance criteria

- [ ] No hardcoded legacy accent hex values (`#B2364C`, `#72BA00`, `#00BFFF`, `#394669`, `#2E3853`, `#1b6ec2`, `#03173d`) remain in CSS/manifest.
- [ ] `:root` design tokens defined in `app.css` and used by both CSS files.
- [ ] Background remains `#000000` and base text remains `#ffffff`.
- [ ] Brand colour `#c000f5` visibly applied to links, headings, button, spinner accent, and console header.
- [ ] `dotnet build` succeeds.
- [ ] Lighthouse CI accessibility ≥ 0.9, best-practices ≥ 0.9.

## 10. Risks / open questions

- Resolved: console header bar stays white with black text (including `<h1>`); brand colour is not used in the header.
- Resolved: optional enhancements (`::selection`, `theme-color` meta) are in scope.
- Resolved: PWA `background_color` matches site background (`#000000`).
- Resolved: `.output-container` border stays invisible (`black`); no border token needed.
- Resolved: `#blazor-error-ui` keeps `lightyellow` (standard error signalling).
- `brand-dark` contrast on black is below AA for small text — enforce usage rules in review.
- Lighthouse `/projects` URL may 404 regardless of this change; out of scope.