---
paths:
  - "src/features/authentication/**"
---

# Auth Screens (Login & Register)

The implemented immersive auth UI. Regenerate to this shape. Complements `auth-flow.md` (state/flow), `styling.md` (CSS conventions), `forms-validation.md`, and `i18n-a11y.md`.

## Shared layout (login and register are identical in size)
- `/login` and `/register` each render a full-viewport `.auth-page` (min-height 100vh, centered) wrapping a `.scene` (max-width 1080px, **min-height 600px**, rounded, `overflow:hidden`).
- `.scene` is two-column: left `.left-content` hero + right `.glass-card` form; **stacks vertically at ≤860px**.
- **Left hero:** brand (inline-SVG ball + "WC 2026 Predictions"), headline "Predict. / **Compete.** (gold) / Win.", tagline, stats row `48 Teams · 104 Matches · 3 Host nations`.
- **Right card:** glassmorphism `.glass-card` (width 360px, **min-height 500px so both screens match — no layout jump**, flex column).
- Background: stadium gradient + pitch-line SVG overlay + top light glow.
- All styles in `features/authentication/pages/auth-scene.css`, **every selector scoped under `.scene`/`.auth-page`** (see `styling.md`).

## Register card (`/register`)
- Title `auth.createAccount`. Fields: **Email**, **Password** (+ `auth.passwordHint`), **Confirm password** — bound to `registerSchema` (email; password mirrors BR-017: ≥8 chars, digit, uppercase, special; `confirmPassword` must match, client-only, never sent).
- Submit (`auth.register`): `useRegister` → `POST /api/auth/register {email,password}` (no token returned) → `navigate("/login", { state: { justRegistered: true } })`.
- Errors: `409` → `setError("email", auth.emailTaken)`; otherwise `applyProblemDetailsToForm` maps server field errors.
- Footer link: `auth.haveAccount` → `/login`.

## Login card (`/login`)
- Title `auth.login`. Fields: **Email**, **Password** — bound to `loginSchema`.
- If navigated with `{ justRegistered: true }` → show `auth.registerSuccess` (green) at the top of the card.
- Submit (`auth.login`): `useLogin` → `authStore.setSession(token)` → `navigate(landingPath())` — **admins land on `/admin`, regular users on `/dashboard`**.
- Errors: `401` → root error `auth.invalidCredentials`; otherwise `applyProblemDetailsToForm`.
- Footer link `auth.noAccount` → `/register`.

> Social sign-in (Google/Facebook) is **not** in the design — OAuth is tier 2; do not add social buttons.

## Conventions
- React Hook Form + `zodResolver`. Inputs have `id` + `<label htmlFor>`, `aria-invalid`, errors in `<span role="alert">`; password hint via `aria-describedby` (see `forms-validation.md`, `i18n-a11y.md`).
- **All visible copy via i18n** (`react-i18next`, locales `en`+`sq`). Keys: `auth.createAccount`, `auth.login`, `auth.register`, `auth.email`, `auth.password`, `auth.passwordHint`, `auth.confirmPassword`, `auth.invalidCredentials`, `auth.emailTaken`, `auth.registerSuccess`, `auth.haveAccount`, `auth.noAccount`.
- No business logic in the page: mutations live in `features/authentication/api` (`useLogin`/`useRegister`); auth state via `lib/auth` (`authStore`/`useAuth`); server-error→field mapping via `lib/forms/applyProblemDetailsToForm`.
