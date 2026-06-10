---
name: create-feature
description: Use when adding a new frontend feature or screen to the World Cup 2026 prediction SPA, given either a free-text description or a path to a specification .md file. Scaffolds a complete feature slice (types, Zod schema, TanStack Query api hooks, presentational components, feature hooks, routes) that conforms to every rule in .claude/rules. Triggers - "new feature", "add a screen", "build the X page", "implement this use case in the UI", "create a feature slice", "scaffold a feature from this spec".
---

# Create Feature

Scaffold a new frontend feature slice that obeys the project rules. Input is **either** a free-text feature description **or** a path to a specification `.md` file.

## 0. Load the rules (mandatory)
Read these before writing anything; they are the contract:
`.claude/rules/feature-slice-architecture.md`, `components-presentation-only.md`,
`server-state-tanstack-query.md`, `api-client-axios.md`, `types-dtos.md`,
`forms-validation.md`, `error-handling.md`, `client-vs-server-responsibility.md`,
`routing-auth.md`, `i18n-a11y.md`, `performance.md`.

## 1. Understand the input
- **If given a `.md` spec**: read the whole file. Extract the feature name, the screen(s)/use case(s), observable behaviour, inputs/outputs, the backend endpoints consumed, validation rules, edge/error conditions, and the auth/role required.
- **If given a description**: restate the use case in one sentence and list the screens, the data it reads, the actions it submits, and the rules the UI must reflect.
- Cross-check against `frontend-architecture.md`, `backend-architecture.md` (the API contract), and `WorldCup2026 BusinessLogic EN.docx` for authoritative domain rules (deadlines, ranges, void cases).

## 2. Plan the slice (decide, then confirm)
Produce a short plan and resolve every choice:
- **Feature & screen names** -> folder `src/features/{feature}/`.
- **Backend slice consumed** -> which endpoints/DTOs (from `backend-architecture.md` §4).
- **Reads vs writes** -> which `useQuery` hooks and which `useMutation` hooks (see `server-state-tanstack-query.md`).
- **Query keys** to add to `lib/api/queryKeys.ts`, and which keys a mutation must **invalidate**.
- **Route(s) & role** -> the `ProtectedRoute` wrapper (`User` / `Admin` / `SuperAdmin`).
- **DTO types** for each request/response (mirror the backend 1:1).
- **Zod schema(s)** for any form (input-shape bounds only, mirroring backend validation).
- **States to design**: loading / empty / error for every async surface; which errors are field-level vs toast (see `error-handling.md`).

## 3. Spec-to-architecture gate (do not guess)
If any behaviour, rule value, DTO field, or boundary is ambiguous or missing, **HALT and ask** before generating code. In particular, confirm the open decisions from `frontend-architecture.md` §11 if they affect this slice (auth model, UI kit, i18n scope). Guessing is a failure. Only proceed once the plan is unambiguous.

## 4. Generate the slice
Create these under `src/features/{feature}/` (see `feature-slice-architecture.md`):

- `types.ts` — explicit `interface`/`type` per request/response, **1:1 with the backend DTO**, camelCase, string enums, no `any` (see `types-dtos.md`).
- `schemas/{useCase}.schema.ts` — Zod schema per form; derive the form type with `z.infer`; bounds mirror backend FluentValidation (goals `0–20` BR-010, etc.) (see `forms-validation.md`).
- `api/{getX}.ts` — typed request functions that call the shared `apiClient` and forward the `AbortSignal` (see `api-client-axios.md`).
- `api/use{X}.ts` — `useQuery` for reads / `useMutation` for writes; keys from the factory; mutations `invalidateQueries` on success (see `server-state-tanstack-query.md`). **No `useEffect` fetching.**
- `components/{Screen}.tsx` — presentational only; consume the hooks; render **loading / empty / error / data** branches; translated copy; accessible markup (see `components-presentation-only.md`, `i18n-a11y.md`). Forms use React Hook Form + `zodResolver` and map API `400` errors to fields via `setError`.
- `hooks/` — any feature-specific non-fetch logic (deriving/formatting). **No business/scoring logic** (see `client-vs-server-responsibility.md`).
- `routes.tsx` — route definitions, each wrapped in the correct `ProtectedRoute`, then wired into `src/app/router.tsx` behind an Error Boundary (see `routing-auth.md`, `error-handling.md`).

Add new query keys to `lib/api/queryKeys.ts` and new i18n keys to the feature's `en`/`sq` namespaces.

## 5. Conformance checklist (verify each)
- Slice self-contained under `src/features/{feature}`; no imports from another feature's internals.
- Components are presentation-only; no Axios/`fetch` and no `useEffect` fetching in components.
- All HTTP via the shared `apiClient`; `AbortSignal` forwarded.
- Reads = `useQuery`, writes = `useMutation`; keys from the factory; correct invalidation.
- DTO types mirror backend 1:1; no `any`; enums as string unions.
- Forms use RHF + Zod; bounds mirror backend; `400` errors mapped to fields.
- Every async surface renders loading / empty / error; ProblemDetails routed correctly.
- Routes behind the right `ProtectedRoute`; no tokens/secrets in client storage or logs.
- User-facing strings translated; semantic, accessible markup.
- No premature memoization; no redundant dependency added.

## 6. Then test it
After the production code compiles, invoke the **write-unit-tests** skill to cover the new slice (schemas, query/mutation hooks, component branches, form error mapping, route guarding).

## Output
Summarize: files created, the backend slice/DTOs consumed, queries vs mutations and the keys invalidated, route + role, i18n keys added, and any assumptions made. List anything you halted on in step 3.
