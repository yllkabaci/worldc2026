# Frontend — World Cup 2026 Predictions

React 18 + TypeScript + **Vite** SPA. Talks to the backend API. Structure and conventions follow
`../../frontend-architecture.md` and `.claude/rules/`.

## Prerequisites
- Node.js 20+

## Setup & run
```bash
npm install
npm run dev        # http://localhost:5173
```
Make sure the backend is running at `http://localhost:5080`:
```bash
dotnet run --project ../backend/src/WorldCup.Api
```
`VITE_API_URL` (in `.env`, default `http://localhost:5080`) points the SPA at the API.

## Scripts
- `npm run dev` — Vite dev server (port 5173)
- `npm run build` — type-check + production build
- `npm run preview` — preview the build
- `npm test` — Vitest
- `npm run typecheck` — `tsc --noEmit`

## Layout
```
src/
  main.tsx
  app/         providers, router, queryClient
  lib/
    api/       client (Axios) + ApiResponse<T> unwrap + ProblemDetails parser + query keys
    auth/      in-memory authStore, useAuth (+ role hierarchy), ProtectedRoute, JWT decode
    logging/   structured logger
    i18n/      i18next (en/sq)
  components/  shared presentational (ErrorBoundary, Skeleton, EmptyState, …)
  features/    one folder per slice (home, authentication, dashboard, …)
```

## Backend connection & CORS
- The SPA runs on `http://localhost:5173`; the backend's CORS policy already allows that origin.
- Auth uses a **Bearer token** (no cookies) → no credentialed CORS needed.
- The Home page calls the backend `/healthz` endpoint to show live backend status — the quickest proof the wiring + CORS work end to end.

## Status (skeleton)
Cross-cutting is wired (API client with envelope unwrap + RFC 7807 parsing, TanStack Query, in-memory
auth + `ProtectedRoute`, i18n, error boundary). **Zero feature screens** beyond Home (health) and a Login
skeleton. Login/auth calls depend on backend **slice 01-auth**; build features via the `create-feature`
skill against `../../specs/features/*`.
