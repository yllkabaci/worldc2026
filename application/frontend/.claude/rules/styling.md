---
paths:
  - "src/**/*.css"
  - "src/**/*.tsx"
---

# Styling

MVP styling is **plain CSS** (no Tailwind / CSS-in-JS — the UI-library choice is deferred). Keep it predictable and leak-free.

## Where styles live
- **Global** resets and small utilities: `src/index.css` (the only place that may define bare global classes like `.container`).
- **Feature/page** styles: a co-located `.css` file imported at the top of the component (e.g. `features/authentication/pages/auth-scene.css` → `import "./auth-scene.css"`).

## No global bleed (critical)
- **Every selector in a feature stylesheet is scoped under a single root class** for that screen (e.g. `.scene .field`, `.scene .btn-main`). **Never** define bare shared names (`.field`, `.error`, `.btn`, `.card`) globally in a feature stylesheet — they would silently override other pages.
- **No inline styles** (`style={{…}}`) — use `className` only.

## Accessibility & responsiveness (WCAG 2.1 AA)
- ≥4.5:1 contrast; visible `:focus`; **min 44×44px** interactive targets.
- Layouts work at **375px**; add breakpoints (e.g. stack a two-column layout at `≤860px`).
- Mark decorative SVG/icons `aria-hidden`; errors use `role="alert"`, status via `role="status"`.

## Consistent dimensions across sibling screens
When two screens share a layout (e.g. **login/register**), pin the shared container and card `min-height` so navigating between them does **not** resize the window. Login and register must render identical scene/card dimensions.

## Tokens currently in use (auth scene)
- Background: stadium gradient `#0a1a0a → #0d2b12 → #061020` + pitch-line SVG (opacity .07) + top light glow.
- Gold accent `#C9A227`; primary action `#2563EB`; glass surface `rgba(255,255,255,0.08)` + `backdrop-filter: blur(20px)`; error `#ff9a9a`; success `#6ee7a0`.
- **Icons:** no icon-font dependency — use inline SVG (or a styled letter for placeholder OAuth marks).
