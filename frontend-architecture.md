# Frontend Architecture Specification

**Project:** World Cup 2026 Prediction App — Frontend SPA
**Audience:** the build agent and the engineering team
**Status:** Architecture baseline (v1.0, Hackathon Edition)
**Stack:** React + TypeScript + Vite (single-page app)
**Companion:** `backend-architecture.md` (the API this SPA consumes). Backend coding conventions: `application/backend/.claude/rules/`. Domain rules: `WorldCup2026 BusinessLogic EN.docx`.

> This document defines **how** the frontend is built. It mirrors the backend's vertical-slice mindset: every backend feature slice has a matching frontend feature folder, and every API DTO has an exact TypeScript type.

---

## 1. Architectural Drivers

- **Contract-first against the API.** The SPA holds **no scoring or business logic** — it renders what the API returns and submits intent. All correctness lives server-side; the UI mirrors validation only for fast feedback (§5).
- **Server state ≠ client state.** Matches, predictions, and leaderboards are *server state* (cached, synchronized via TanStack Query). Auth/session and UI toggles are *client state*.
- **Resilient UX.** Every async surface has loading, empty, and error states; runtime crashes are contained by error boundaries.
- **Two trust tiers + locale.** User vs Admin (vs Super Admin) gate routes; the app ships English/Albanian (business doc §9.1).

---

## 2. Key Architectural Decisions

| # | Decision | Choice | Rationale |
|---|----------|--------|-----------|
| FE-1 | Build tool | **Vite** + React 18 + TypeScript (strict) | Fast, modern SPA baseline. |
| FE-2 | Folder organisation | **Feature-based** folders mirroring backend slices | Vertical-slice parity; features stay independent. |
| FE-3 | Server state | **TanStack Query** for all fetching/mutations/caching | No `useEffect` data fetching. |
| FE-4 | HTTP client | **Axios** with a centralised instance + interceptors | JWT attach, error parsing, abort. |
| FE-5 | Forms + validation | **React Hook Form** + **Zod** | Performant forms; schemas mirror backend FluentValidation. |
| FE-6 | Routing | **React Router** with role-aware `ProtectedRoute` wrappers | Matches backend authorization policies. |
| FE-7 | Error model | **RFC 7807 ProblemDetails** parser → field errors + toasts | Exact symmetry with `backend-architecture.md` §7. |
| FE-8 | Crash containment | **React Error Boundaries** per route subtree | Localised fallback UIs. |
| FE-9 | i18n | **react-i18next** (en/sq) | Required locales. |
| FE-10 | Styling/UI | A utility or component kit (e.g. Tailwind, or MUI/shadcn) with skeleton support | Consistent loading/empty states. *Confirm in §11.* |

---

## 3. Folder Structure

```
/application/frontend
  index.html
  vite.config.ts
  tsconfig.json            # "strict": true, no implicit any
  .env                     # VITE_API_URL=...
  src/
    main.tsx               # app bootstrap: QueryClientProvider, Router, i18n, ErrorBoundary
    app/
      router.tsx           # route tree + ProtectedRoute composition
      providers.tsx        # QueryClient, Auth, i18n, Toast providers
      queryClient.ts       # TanStack Query defaults (retry, staleTime)
    lib/
      api/
        client.ts          # Axios instance + interceptors (JWT, abort, error parse)
        problemDetails.ts  # RFC 7807 types + parser
        queryKeys.ts       # centralised query key factory
      auth/
        authStore.ts       # session/token state (client state)
        useAuth.ts
      logging/
        logger.ts          # structured UI event logging
      i18n/
    components/            # SHARED, presentational only (Button, Skeleton, EmptyState, Toast)
    features/              # VERTICAL SLICES — one folder per backend slice
      authentication/
        api/               # query/mutation hooks (useLogin, useRegister)
        components/        # LoginForm, RegisterForm (presentation)
        hooks/             # feature-specific logic
        schemas/           # Zod schemas (mirror FluentValidation)
        types.ts           # request/response DTO types (map to backend DTOs)
        routes.tsx
      predictions/
      matches/
      leaderboard/
      groups/
      profile/
      admin/
  tests/
```

**Rule:** components are **presentation only**. Data fetching, mutations, and logic live in `features/<x>/api` (TanStack Query hooks) and `features/<x>/hooks`. A component never calls Axios directly.

---

## 4. Feature Slices (mirror the backend)

Each frontend feature consumes the matching backend slice (`backend-architecture.md` §4):

| Feature folder | Consumes (backend) | Key screens |
|----------------|--------------------|-------------|
| `authentication` | Register / Login / OAuth / ResetPassword | Login, Register |
| `profile` | UpdateProfile / GetMyHistory | Profile, My History |
| `predictions` | MakePrediction / ModifyPrediction / GetMyActivePredictions | Prediction form, My predictions |
| `matches` | GetMatchCalendar / GetMatchDetails / GetGroupStandings | Calendar, Match detail, Group tables |
| `leaderboard` | GetGlobalLeaderboard / GetMyRanking | Global board, My ranking |
| `groups` | CreateGroup / JoinGroup / GetGroupLeaderboard | Create/Join, Group board |
| `admin` | SetOfficialResult / Config / Users / Reports | Result entry, Config, User mgmt |

---

## 5. Types, Forms & Validation (contract symmetry)

- **TypeScript-first, strict, no `any`.** Every request/response has an explicit type in `features/<x>/types.ts` that maps 1:1 to the backend DTO. Treat the backend as the contract owner; it exposes **Swagger/OpenAPI at `/swagger/v1/swagger.json`** (Development) — generate types from it, or hand-mirror and keep in sync. Note: the backend wraps success payloads in an **`ApiResponse<T>`** envelope, so types model the **inner** payload `T` and the client unwraps the envelope (see §6).
- **Functional components** with explicit `interface Props {}`. No class components (except the Error Boundary).
- **React Hook Form + Zod**: each form has a Zod schema that **mirrors the backend FluentValidation rules** so the user gets instant feedback, e.g. prediction goals `0–20` integer (BR-010), yellow cards `0–20` (BR-024), red cards `0–10` (BR-025), subs `0–5` per team (BR-028). Client validation is a UX convenience — **the server remains the source of truth**.
- **Performance:** apply `useMemo`/`useCallback`/`React.memo` only where a measured re-render problem exists (e.g. large leaderboard lists) — not by default.

---

## 6. API Communication & State

- **Centralised Axios instance** (`lib/api/client.ts`) with interceptors that:
  1. attach the JWT bearer token to every request,
  2. forward the TanStack Query `AbortSignal` so unmount/navigation cancels in-flight requests,
  3. parse non-2xx responses into a typed `ProblemDetails` (§7) before they reach hooks,
  4. handle `401` globally (clear session → redirect to login),
  5. **unwrap the backend's `ApiResponse<T>` success envelope** so hooks/components receive the inner DTO directly (failures stay as RFC 7807 ProblemDetails, never the envelope).
- **TanStack Query owns all server state.** Queries for reads (calendar, leaderboard, my predictions); mutations for writes (make/modify prediction, admin set-result). After a mutation, **invalidate** the affected query keys (e.g. settling/predicting invalidates `leaderboard` and `predictions`). **No `useEffect`-based fetching.**
- **Query key factory** in `lib/api/queryKeys.ts` for consistent caching/invalidation.
- Sensible defaults: `staleTime` per resource (calendar longer, leaderboard shorter), retry off for `4xx`.

---

## 7. Error Handling (RFC 7807)

- `lib/api/problemDetails.ts` defines the `ProblemDetails` type (incl. the `errors` dictionary for validation) matching the backend's `IExceptionHandler` output.
- The parser routes errors by shape:
  - **Validation (`400` with `errors`)** → mapped onto the corresponding React Hook Form fields via `setError`, so messages appear inline.
  - **Domain conflicts (`409`/`422`, e.g. predicting after the deadline, BR-007)** → a descriptive **toast**.
  - **`401`** → session cleared, redirect to login. **`403`** → "not authorised" view.
  - **`5xx`/unknown** → generic toast + logged event.
- **Error Boundaries** wrap each route subtree (and a top-level fallback) to catch render-time crashes and show a recoverable fallback UI without taking down the whole app.

---

## 8. Security & Routing

- **Token storage:** recommended — keep the **access token in memory** and use an **HttpOnly refresh cookie**; for the hackathon a simpler in-memory token (accepting re-login on refresh) is acceptable. *Confirm in §11; this depends on the backend auth decision.*
- **`ProtectedRoute` wrappers** read auth state and gate by role, mirroring backend policies: `RequireUser`, `RequireAdmin`, `RequireSuperAdmin`. Unauthorised access redirects (login) or shows a `403` view. Admin screens live behind `RequireAdmin`.
- Never store secrets in client state; never log tokens.

---

## 9. Observability & UX

- **Structured UI logging** (`lib/logging/logger.ts`): log critical user actions with structured properties — `logger.event("prediction_submitted", { matchId })`, plus failed network requests. Keep it semantic, not string-concatenated. Optionally adopt the **OpenTelemetry browser SDK** and propagate trace headers so UI spans join the backend traces (`backend-architecture.md` §8).
- **Loading / empty / error states everywhere:** Skeletons for lists (calendar, leaderboard), spinners for mutations, and descriptive empty states ("No upcoming matches", "You haven't made any predictions yet"). No raw blank screens.
- **Request cancellation:** TanStack Query's `AbortSignal` is wired into Axios so navigating away or unmounting aborts in-flight requests (the FE counterpart to the backend's `CancellationToken`).

---

## 10. Hackathon Scope & Build Order

Mirror the backend spine (`backend-architecture.md` §12) — build the verifiable core first:

1. **Core (must):** auth (login), matches calendar, make/modify prediction with the deadline rule + Zod validation, global leaderboard with tiebreak display.
2. **Second tier (if time):** my-predictions history, match detail + group standings, admin result-entry screen, points-config screen.
3. **Defer:** private groups, notifications, analytics/export, OAuth buttons, i18n polish, share-as-image.

Keep the UI thin: the demo-able loop is *see fixtures → predict → (admin settles) → leaderboard updates*.

---

## 11. Open Decisions / Assumptions (align with backend §13)

1. **Token storage / auth model** — in-memory + HttpOnly refresh vs simple in-memory for the hackathon; depends on the backend auth decision (local JWT vs OAuth).
2. **UI library** — Tailwind vs a component kit (MUI/shadcn). Pick one for consistent skeletons/toasts.
3. **Type generation** — generate TS types from the backend Swagger/OpenAPI (`/swagger/v1/swagger.json`), or hand-mirror DTOs?
4. **Bonus-prediction UI** — include all seven bonus inputs in the prediction form, or start with exact-score only (matching the backend MVP tier)?
5. **i18n in MVP** — ship en/sq from the start or English-only for the hackathon?

> Resolve these with the backend decisions before building, so the contract (types, auth, validation) stays symmetric on both sides.

---

## 12. Screens Inventory

*(Harvested from the product spec; maps onto the feature folders in §3–§4. Tier tags reflect the MVP scope.)*

**User-facing**

| # | Screen | Key elements | Tier |
|---|--------|--------------|------|
| 1 | Home / Live feed | Upcoming matches (next 7 days), quick-predict CTA per match | spine |
| 2 | Match detail | Score, venue, prediction input (main score; bonus fields tier 2) | spine |
| 3 | Prediction slip | Active predictions, edit within deadline, submit | spine |
| 4 | Global leaderboard | Filters (period/stage/country), current-user rank pinned | spine |
| 5 | Group leaderboard | Private ranking, members only | tier 2 |
| 6 | Match calendar | All 104 matches, filter by group/team/date/stage | spine |
| 7 | Group-stage standings | 12 groups (A–L), points/goals/position | spine |
| 8 | Profile | Stats, prediction history, rank chart, editable fields | tier 2 |
| 9 | Groups | Create/join via 6-char invite code, manage members | tier 2 |
| 10 | Notifications center | Reminders, deadline warnings, result/rank alerts | tier 2 |

**Admin-facing**

| # | Screen | Key elements | Tier |
|---|--------|--------------|------|
| 11 | Admin dashboard | Analytics: users, daily predictions, accuracy, active groups | tier 2 |
| 12 | Match management | Set official results, cancel/postpone, audited re-settlement | spine |
| 13 | Business-rules config | Points, multipliers, prediction windows | tier 2 |
| 14 | User management | List/filter, block/activate | tier 2 |
| 15 | Tournament config | Teams, groups, schedule, stadiums | tier 2 |
| 16 | API sync config | Football API key, sync interval, manual override | tier 2 |

## 13. Accessibility & UX Standards (WCAG 2.1 AA)

- Color contrast ≥ 4.5:1 for text; never use color as the only indicator (pair with icon/label).
- ARIA labels on icons, buttons, score displays, and prediction inputs; ARIA live regions for any real-time updates.
- Visible focus states on all interactive elements; honor `prefers-reduced-motion`.
- Minimum tap target 44×44px; base design at 375px, scaling to 768px and 1280px.
- Skeleton screens on every load; inline prediction confirmation (no full-page redirects); plain-language errors with a resolution path; deadline countdown on match cards.

## 14. SEO (public pages)

- SSR/SSG for public pages (calendar, standings, match detail).
- JSON-LD `schema.org/SportsEvent` per match; Open Graph + Twitter Card meta.
- Title pattern: `France vs Argentina — Group C — World Cup 2026 Predictions`.
- Core Web Vitals targets: LCP < 2.5s, CLS < 0.1, INP < 200ms.

## 15. Notifications & Real-time (tier 2)

Real-time score/leaderboard push and the notifications center are **tier 2** (not in the MVP spine, which is REST + poll/invalidate via TanStack Query). When built, triggers are: 24h before a match (users who haven't predicted), 1h before the window closes (open predictions), result confirmed (users who predicted), and rank change (that user). A real-time transport (e.g. SignalR) would be introduced here — it is **not** part of the MVP and is not yet reflected in the backend rules.
