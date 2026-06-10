---
paths:
  - "src/features/**/types.ts"
  - "src/features/**/*.types.ts"
---

# DTO & Type Conventions

The frontend is **contract-first** against the API. Every request/response shape has an explicit TypeScript type in `features/<x>/types.ts` that maps **1:1 to the backend DTO** (backend `dtos-records.md`). The backend owns the contract; if it exposes OpenAPI, generate types from it — otherwise hand-mirror and keep in sync.

## Request & Response types
```ts
// features/predictions/types.ts

/** Submits a prediction for a match. Mirrors MakePredictionRequest (backend). */
export interface MakePredictionRequest {
  homeGoals: number;   // BR-010: 0-20
  awayGoals: number;   // BR-010: 0-20
}

/** Mirrors MakePredictionResponse (backend). */
export interface MakePredictionResponse {
  predictionId: string;
  matchId: string;
  submittedAt: string; // ISO-8601
}
```

## Rules
- **No `any`** — explicit or implicit. Strict TypeScript stays on (`tsconfig` `"strict": true`); rules may not relax it. Use `unknown` + narrowing, generics, or precise types.
- **Explicit interface/type per DTO**, named to match the backend: `{UseCase}Request`, `{UseCase}Response`, `{UseCase}ListResponse`.
- **camelCase** fields — the backend serialises camelCase with string enums (backend `json-serialization.md`), so types mirror that exactly. Enums are string union types, not numbers.
- **Strongly-typed ids** arrive as their underlying value (e.g. `matchId: string`); don't model them as nested objects.
- **Dates** are ISO-8601 `string`s as received; parse to `Date` only at the edge where needed.
- **No type assertions** (`as Foo`) to silence the compiler where a real fix exists. `as const` and narrowing helpers are fine.
- **No logic in types** — pure data shapes. Derive the form value type from the Zod schema with `z.infer` (see `forms-validation.md`) so schema and type can't drift.

## Keeping in sync
- When a backend DTO changes, the matching TS type changes in the same PR (or types are regenerated from OpenAPI).
- A response type is the payload **inside** the backend's `ApiResponse<T>` envelope — model the envelope once in `lib/api`, the feature type is just the payload.
