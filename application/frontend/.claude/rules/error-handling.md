---
paths:
  - "src/lib/api/problemDetails.ts"
  - "src/**/*ErrorBoundary*.tsx"
  - "src/**/*errorHandling*.ts"
---

# Error Handling (RFC 7807)

The backend signals failures as **RFC 7807 ProblemDetails** with typed error codes (backend `error-codes.md`). The frontend parses every non-2xx into a typed `ProblemDetails` in `lib/api/problemDetails.ts` and routes it by shape. Hooks and components **react** to errors; they don't build them by hand.

## ProblemDetails type
```ts
// lib/api/problemDetails.ts - matches the backend IExceptionHandler output
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errorCode?: string;                 // backend WC-NNNN extension member
  errors?: Record<string, string[]>;  // field -> messages (validation)
}
```

## Routing by shape
| Response | Frontend handling |
|----------|-------------------|
| `400` with `errors` | map onto React Hook Form fields via `setError` — inline messages (see `forms-validation.md`) |
| `409` / `422` domain conflict (e.g. predicting after deadline, BR-007) | descriptive **toast** |
| `401` | session cleared → redirect to login (handled in the Axios interceptor, see `api-client-axios.md`) |
| `403` | "not authorised" view |
| `5xx` / unknown | generic toast + logged event (`lib/logging/logger.ts`) |

This table is the FE mirror of the backend's code→HTTP map; keep the two symmetric.

## Error Boundaries (render-time crashes)
- Each route subtree is wrapped in a React **Error Boundary** with a recoverable fallback, plus a top-level fallback. A render crash in one screen must not take down the app.
- The Error Boundary is the **only** permitted class component (see `components-presentation-only.md`).

```tsx
<ErrorBoundary fallback={<RouteErrorFallback />}>
  <PredictionsRoutes />
</ErrorBoundary>
```

## Rules
- **Never swallow errors** — every `useQuery`/`useMutation` surfaces an error branch (see `components-presentation-only.md`).
- Validation errors are **field-level**, not toasts; domain conflicts are **toasts**, not field errors.
- Never expose raw stack traces or `5xx` detail to the user; log them via `lib/logging/logger.ts` with structured properties (`logger.event("prediction_failed", { matchId })`).
- The parser lives in `lib/api`; features import it — they don't re-parse Axios errors themselves.
