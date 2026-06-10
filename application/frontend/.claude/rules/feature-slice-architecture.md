---
paths:
  - "src/features/**/*"
  - "src/components/**/*"
---

# Feature Slice Architecture

The SPA mirrors the backend's vertical slices. Every backend feature slice has a matching **frontend feature folder**, and each slice is self-contained: it owns its **api hooks, components, feature hooks, Zod schemas, types, and routes**, co-located in one folder under `src/features/`.

## Feature Slice Structure
```
src/features/{feature}/
  api/          # TanStack Query hooks - the ONLY place HTTP is touched (see server-state-tanstack-query.md)
  components/   # presentational React components (see components-presentation-only.md)
  hooks/        # feature-specific non-fetch logic
  schemas/      # Zod schemas, mirror backend FluentValidation (see forms-validation.md)
  types.ts      # request/response DTO types, 1:1 with backend (see types-dtos.md)
  routes.tsx    # route definitions + ProtectedRoute wiring (see routing-auth.md)
```

The feature folders mirror the backend slices: `authentication`, `profile`, `predictions`, `matches`, `leaderboard`, `groups`, `admin`.

## What lives where
- **`src/features/<x>`** — one slice per backend feature. Self-contained; consumes the matching backend slice.
- **`src/components`** — SHARED, generic, presentational components only (Button, Skeleton, EmptyState, Toast). If a component knows about predictions or matches, it belongs in a feature.
- **`src/lib`** — cross-cutting infrastructure: `api/` (Axios client, ProblemDetails, query-key factory), `auth/`, `logging/`, `i18n/`.
- **`src/app`** — bootstrap: `router.tsx`, `providers.tsx`, `queryClient.ts`.

## Slice Independence
- A feature must **not** import from another feature's internals. Share via `src/components` or `src/lib`.
- Use cases sit **directly** under the feature folder. Do not add intermediate grouping directories (`Pages/`, `Containers/`) between the feature and its files.

## Adding a New Feature
1. Create `src/features/{feature}/` with the folders above.
2. Add `routes.tsx` and wire it into `src/app/router.tsx` behind the correct `ProtectedRoute` (see `routing-auth.md`).
3. Put DTO types in `types.ts` (see `types-dtos.md`), fetching in `api/` (see `server-state-tanstack-query.md`), forms+schemas per `forms-validation.md`.
4. Components stay presentation-only (see `components-presentation-only.md`); no business logic on the client (see `client-vs-server-responsibility.md`).
