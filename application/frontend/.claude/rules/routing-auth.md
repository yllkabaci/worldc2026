---
paths:
  - "src/app/router.tsx"
  - "src/features/**/routes.tsx"
  - "src/lib/auth/**/*"
---

# Routing & Auth

Routing uses **React Router**. Role-gated screens sit behind `ProtectedRoute` wrappers that mirror the backend authorization policies (backend `minimal-api-endpoints.md` — `RequireAuthorization("User"|"Admin"|"SuperAdmin")`). The client gate is UX; the server still enforces the policy on every request.

## ProtectedRoute
```tsx
// lib/auth/ProtectedRoute.tsx
interface Props { role: "User" | "Admin" | "SuperAdmin"; children: ReactNode; }

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
- **Every protected route** is wrapped in the correct `ProtectedRoute` mapping to the backend policy: `RequireUser` / `RequireAdmin` / `RequireSuperAdmin`. Admin screens go behind `Admin`.
- **Unauthorised** access redirects to login (`401` shape) or shows a `403` view — never silently renders the screen.
- **Auth state lives in `lib/auth`** (`authStore`, `useAuth`); features read it, never reinvent it.
- **Token storage:** access token in **memory** with an **HttpOnly refresh cookie** is preferred; a simpler in-memory token (re-login on refresh) is acceptable for the hackathon. Confirm against the backend auth decision (architecture §8/§11).
- **Never** store tokens or secrets in `localStorage`/`sessionStorage` unless the agreed model explicitly allows it; **never** log tokens.
- Each route subtree is wrapped in an **Error Boundary** (see `error-handling.md`).
- `401` handling (clear session + redirect) is centralised in the Axios interceptor (see `api-client-axios.md`), not duplicated per route.
