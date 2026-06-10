---
paths:
  - "src/lib/auth/**/*"
  - "src/features/authentication/**/*"
---

# Auth Flow (Client)

How authentication works on the client. MVP is **minimal auth** (mirrors the backend decision): email/password → a JWT the backend issues; the client holds the **access token in memory only**, decodes its claims for role gating (UX), and the backend enforces every policy. **No refresh token in the MVP** — on a full page reload the session is gone and the user logs in again. OAuth, refresh cookies, lockout, and email verification are tier 2.

## Auth state (`lib/auth/authStore`)
- Holds the **in-memory** access token and the derived current user (`id`, `email`, `roles`). **Never** persisted to `localStorage`/`sessionStorage`.
- API: `getToken()`, `setSession(token)`, `clear()`, `clearAndRedirect(to = "/login")`.
- `setSession` decodes the JWT payload to populate the current user (no extra round-trip).
```ts
// lib/auth/decodeToken.ts — read claims for UX gating only (server still enforces)
interface JwtClaims { sub: string; email: string; role: string | string[]; exp: number; }
```

## `useAuth()` (`lib/auth/useAuth`)
Exposes `{ isAuthenticated, user, roles, hasRole, login, logout }`. Features read auth **only** through this hook — they never touch the store directly or re-decode tokens.

### Role hierarchy (mirror backend policies)
`hasRole` honours the backend hierarchy (see backend `auth-and-authorization.md`): `User` is satisfied by `User|Admin|SuperAdmin`; `Admin` by `Admin|SuperAdmin`; `SuperAdmin` by `SuperAdmin`.

## Login
- `useLogin` mutation → `POST /api/auth/login` → on success `authStore.setSession(token)` then navigate to the intended route (or `/`).
- The login form uses **React Hook Form + Zod** (email + password). A `401` maps to a form-level error/toast (no field disclosure — see `error-handling.md`, `forms-validation.md`).

## Register
- `useRegister` mutation → `POST /api/auth/register`. On success, either auto-login (call the login flow) or redirect to `/login`.
- The Zod schema mirrors the backend password rule (BR-017: ≥8 chars, ≥1 digit, ≥1 uppercase, ≥1 special) for instant feedback — **the server remains the source of truth**. Duplicate email → `409` surfaced as a form error.

## Protected routes
- Role-gated screens sit behind `ProtectedRoute` (see `routing-auth.md`), which reads `useAuth()` and maps to the backend policy. Unauthenticated → redirect to `/login`; wrong role → `403` view. The client gate is **UX only**; the server enforces on every request.

## 401 / expiry / logout
- A `401` from any request is handled **once**, centrally, in the Axios interceptor: `authStore.clearAndRedirect()` (see `api-client-axios.md`). No per-screen handling, no silent refresh in the MVP.
- **Logout:** clear the in-memory session, **clear the TanStack Query cache** (`queryClient.clear()`), redirect to `/login`.
- **Reload:** because the token is memory-only, a hard refresh logs the user out — they re-authenticate. (Tier 2: an HttpOnly refresh cookie would silently restore the session.)

## Security
- **Never** store the token in `localStorage`/`sessionStorage`; **never** log the token or decoded claims; clear everything on logout. Role gating is convenience — authority is the backend policy.

## Testing
- Unit-test `hasRole` hierarchy and `decodeToken`. Test `ProtectedRoute` redirects (unauthenticated → login, wrong role → 403). Test login success (session set, redirect) and failure (`401` → error, no session). Inject a fake `authStore`/token; never call a real backend (see `testing-conventions.md`).

## Forbidden
No token in web storage; no logging tokens/claims; no client-only authorization treated as security; no bespoke `fetch` for auth calls (use the api-client); no re-decoding tokens outside `lib/auth`.
