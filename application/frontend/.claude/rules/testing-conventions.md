---
paths:
  - "tests/**/*"
  - "src/**/*.test.ts"
  - "src/**/*.test.tsx"
---

# Testing Conventions

Tests run on **Vitest** + **React Testing Library** (RTL), with **MSW** (Mock Service Worker) for the network boundary and **jsdom** as the environment. Test behaviour through the public surface (what the user sees / what a hook returns), not implementation details.

## Coverage matrix

| Target | What to cover | How |
|--------|---------------|-----|
| **Zod schema** (`schemas/`) | valid input passes; one failing case per rule (each range/required, BR bounds) | call `schema.safeParse(...)` directly — no React |
| **Query hook** (`api/use*`) | success → `data` shape; error → `isError` + parsed `ProblemDetails` | `renderHook` + QueryClient wrapper + MSW |
| **Mutation hook** (`api/use*`) | success path; invalidates the right query keys; error surfaces `ProblemDetails` | `renderHook` + MSW + spy on `invalidateQueries` |
| **Component** | renders **loading / empty / error / data** branches; user interactions fire the right callbacks | RTL `render` + `userEvent` |
| **Form** | valid submit calls mutation; invalid shows inline errors; API `400` maps to fields via `setError` | RTL + `userEvent` + MSW |
| **Error mapping** (`problemDetails.ts`) | each status (400/401/403/409/5xx) routes to the right outcome | unit test the parser directly |
| **ProtectedRoute** | unauthenticated → redirect; wrong role → 403 view; authorised → children | RTL with a mocked `useAuth` |

## Conventions
- **AAA**: structure every test with `// Arrange / // Act / // Assert`.
- **Naming**: `describe("useMakePrediction", ...)` + `it("invalidates leaderboard on success", ...)` — describe the behaviour, not the method signature.
- **Query semantics over test ids**: prefer `getByRole` / `getByLabelText` / `getByText`; use `data-testid` only as a last resort. This doubles as an accessibility check (see `i18n-a11y.md`).
- **`userEvent`** (not `fireEvent`) for interactions.
- **MSW** mocks HTTP at the network layer — do **not** mock `apiClient` or Axios directly. Define handlers per feature and override per test for error cases.
- **Factories**: reuse `tests/factories/{entity}Factory.ts` (e.g. `makeMatch(overrides)`); check for an existing factory before inlining fixtures.
- **Async**: assert with `findBy*` / `waitFor`; never a fixed `setTimeout`.
- One test file per implementation file, co-located as `X.test.ts(x)` next to `X`, or mirrored under `tests/`.

## TanStack Query test wrapper
```tsx
function renderWithClient(ui: ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}
```
- **`retry: false`** in tests so error cases fail fast.
- Fresh `QueryClient` per test — no shared cache leaking between tests.

## What to test vs not
- **Test**: branch logic, error routing, schema bounds, invalidation, conditional rendering, accessibility roles.
- **Don't test**: third-party internals (React Router, TanStack Query), or that a presentational component renders a static string — cover behaviour, not snapshots of markup.
- Mirror the server contract: when a test asserts a validation message, it reflects the backend rule (see `forms-validation.md`), keeping FE/BE symmetric.

## Run them
Run `npm run test` (or `npx vitest run`) for the affected files. The task is not done while tests fail or a target is only partially covered.
