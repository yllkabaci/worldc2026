---
name: write-unit-tests
description: Use when writing or adding tests for the World Cup 2026 frontend - a Zod schema, a TanStack Query hook (query or mutation), a presentational component, a form, the ProblemDetails error mapper, or a ProtectedRoute. Produces tests that conform to .claude/rules/testing-conventions.md (Vitest, React Testing Library, MSW, factories, AAA) and the rule for the code under test. Triggers - "write tests for", "unit test this", "add tests", "test the hook/component/schema/form", "cover this feature with tests".
---

# Write Unit Tests

Write tests for a target (a feature slice, a query/mutation hook, a component, a form, the error mapper, or a route guard), following the project conventions exactly.

## 0. Load the rules (mandatory)
Always read `.claude/rules/testing-conventions.md`. Then read the rule for the code under test:
- Zod schema / form → `forms-validation.md`
- Query / mutation hook → `server-state-tanstack-query.md`, `api-client-axios.md`
- Component → `components-presentation-only.md`, `i18n-a11y.md`
- Error mapping → `error-handling.md`
- ProtectedRoute / routing → `routing-auth.md`
- Anything asserting a business outcome → `client-vs-server-responsibility.md` (the client reflects, it does not compute)

## 1. Identify the target layer(s)
Read the code under test and classify it. Use the coverage matrix from `testing-conventions.md`:

| Target | What to cover |
|--------|---------------|
| **Zod schema** | valid input passes; one failing test per rule (each range/required; BR bounds) |
| **Query hook** | success → `data` shape; error → `isError` + parsed `ProblemDetails` |
| **Mutation hook** | success path; **invalidates the correct query keys**; error surfaces `ProblemDetails` |
| **Component** | loading / empty / error / data branches; interactions fire the right callbacks |
| **Form** | valid submit calls the mutation; invalid shows inline errors; API `400` maps to fields via `setError` |
| **Error mapper** | each status (400+`errors` / 401 / 403 / 409 / 5xx) routes to the right outcome |
| **ProtectedRoute** | unauthenticated → redirect; wrong role → 403 view; authorised → renders children |

## 2. Conventions to apply
- **AAA** with `// Arrange / // Act / // Assert` comments.
- **Vitest** runner; **React Testing Library** for components/hooks; **`userEvent`** for interactions (not `fireEvent`).
- **MSW** mocks the network — never mock `apiClient`/Axios directly. Override handlers per test for error cases.
- **Query by role/label/text** (`getByRole`, `getByLabelText`, `findByText`); `data-testid` only as a last resort.
- **Naming**: `describe("<unit>")` + `it("<behaviour>")` — describe behaviour, not signatures.
- Reuse/extend factories in `tests/factories/{entity}Factory.ts`; check existing factories before inlining fixtures.
- One test file per implementation file; co-locate as `X.test.ts(x)`.

## 3. Layer-specific guidance

### Zod schemas (fast, do first)
- Call `schema.safeParse(...)` directly — no React, no MSW.
- One passing case; one failing case **per rule**, asserting the failing path/message. Sweep ranges with `it.each` (e.g. `-1`, `0`, `20`, `21` for goals 0–20, BR-010).

### Query / mutation hooks
- Render with the QueryClient wrapper from `testing-conventions.md` (`retry: false`, fresh client per test).
- Drive HTTP with **MSW**: a success handler and, per error test, an override returning `400`/`409`/`5xx` with a `ProblemDetails` body.
- For mutations, assert it **invalidates the right keys** (spy on `queryClient.invalidateQueries`) and that errors reject as parsed `ProblemDetails`.

### Components
- Render each branch: mock the hook (or MSW state) to produce **loading → empty → error → data**; assert the right UI for each (skeleton, empty copy, error message, rows). No bare blank screen.
- Assert interactions via `userEvent` and that callbacks/mutations are invoked.

### Forms
- Submit valid data → assert the mutation is called with the mapped command.
- Submit invalid data → assert inline field errors appear (from Zod).
- Simulate an API `400` with an `errors` dictionary (MSW) → assert messages land on the correct fields via `setError`, not as a toast.

### Error mapper
- Unit-test `parseProblemDetails` / the routing table directly: feed each status shape, assert the resulting action (field errors / toast / redirect / generic).

### ProtectedRoute
- Render inside a `MemoryRouter` with `useAuth` mocked for: unauthenticated (redirect to `/login`), authenticated wrong role (403 view), authorised (children render).

## 4. Run them
Run `npm run test` (or `npx vitest run`) for the affected files. Do not consider the task done while tests fail or the target is only partially covered.

## Output
Summarize: files added, the layers covered, factory methods added/reused, MSW handlers used, and the `vitest` result.
