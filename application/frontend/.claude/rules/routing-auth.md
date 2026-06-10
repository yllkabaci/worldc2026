---
paths:
  - "src/app/router.tsx"
  - "src/features/**/routes.tsx"
  - "src/lib/auth/**/*"
---

# Routing & Auth

Routing uses **React Router**. Role-gated screens sit behind `ProtectedRoute` wrappers that mirror the backend authorization policies (backend `minimal-api-endpoints.md` — `RequireAuthorization("User"|"Admin")`). The client gate is UX; the server still enforces the policy on every request.

## ProtectedRoute
```tsx
// lib/auth/ProtectedRoute.tsx
interface Props { role: "User" | "Admin"; children: ReactNode; }

export function ProtectedRoute({ role, children }: Props) {
  const { isAuthenticated, hasRole } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (!hasRole(role)) return <ForbiddenView />; // 403 view
  return <>{children}</>;
}
```

## Route composition
```tsx
// app/router.tsx
{ path: "/predictions", element: <ProtectedRoute role="User"><PredictionsRoutes /></ProtectedRoute> },
{ path: "/admin",       element: <ProtectedRoute role="Admin"><AdminRoutes /></ProtectedRoute> },
```

## Rules
- **Every protected route** is wrapped in the correct `ProtectedRoute` mapping to the backend policy: `User` (any authenticated) or `Admin` (the user's `IsAdmin`). Admin screens go behind `Admin`.
- **Unauthorised** access redirects to login (`401` shape) or shows a `403` view — never silently renders the screen.
- **Auth state lives in `lib/auth`** (`authStore`, `useAuth`); features read it, never reinvent it.
- **Token storage (decided):** access token in **memory only** for the MVP — no refresh token, so a full reload logs the user out and they re-authenticate (mirrors the backend minimal-auth decision). An HttpOnly refresh cookie is **tier 2**. Full flow in `auth-flow.md`.
- **Never** store tokens or secrets in `localStorage`/`sessionStorage` unless the agreed model explicitly allows it; **never** log tokens.
- Each route subtree is wrapped in an **Error Boundary** (see `error-handling.md`).
- `401` handling (clear session + redirect) is centralised in the Axios interceptor (see `api-client-axios.md`), not duplicated per route.
- **Post-login landing by role:** after a successful login the app routes by the user's role — **admins → `/admin`, regular users → `/dashboard`** — via `landingPath()` (`lib/auth`), which reads the token's roles. `landingPathForRoles(roles)` is the pure, unit-tested form.
