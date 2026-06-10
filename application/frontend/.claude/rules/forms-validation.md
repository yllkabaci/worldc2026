---
paths:
  - "src/features/**/schemas/**/*.ts"
  - "src/features/**/components/**/*Form*.tsx"
---

# Forms & Validation (React Hook Form + Zod)

Forms use **React Hook Form** resolved by a **Zod** schema. Each schema **mirrors the backend FluentValidation rules** (backend `fluent-validation.md`) so the user gets instant feedback. Client validation is a **UX convenience only** — the server remains the source of truth (see `client-vs-server-responsibility.md`).

## Schema (mirrors backend input bounds)
```ts
// features/predictions/schemas/makePrediction.schema.ts
export const makePredictionSchema = z.object({
  homeGoals: z.number().int().min(0).max(20),  // BR-010
  awayGoals: z.number().int().min(0).max(20),  // BR-010
});

export type MakePredictionForm = z.infer<typeof makePredictionSchema>;
```

## Form wiring
```tsx
const form = useForm<MakePredictionForm>({ resolver: zodResolver(makePredictionSchema) });
const makePrediction = useMakePrediction(); // mutation hook (see server-state-tanstack-query.md)

const onSubmit = form.handleSubmit((values) =>
  makePrediction.mutate(values, {
    onError: (err) => applyProblemDetailsToForm(err, form.setError), // see error-handling.md
  }),
);
```

## Rules
- **RHF + Zod** for every form; resolve with `zodResolver`. Derive the form type with `z.infer` — never hand-write a parallel type.
- **Schemas mirror backend bounds exactly**: goals `0–20` (BR-010). When a business rule changes, the schema changes with it.
- **Input shape only.** Like the backend validator, Zod checks types, required, ranges, formats — **never** stateful/business rules ("deadline passed", "one prediction per match", scoring). Those are enforced server-side and surfaced as errors (see `error-handling.md`).
- **Map API validation errors back to fields.** A `400` with an `errors` dictionary is applied via `form.setError`, so messages appear inline — not as a generic toast.
- **Guard against double-submit** — disable submit / use the mutation's pending state.
- **Label every input** and associate errors for accessibility (see `i18n-a11y.md`).
