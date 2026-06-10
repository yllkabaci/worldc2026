---
paths:
  - "src/lib/api/**/*.ts"
---

# API Client (Centralised Axios)

There is **one** Axios instance, in `lib/api/client.ts`. Feature `api/` functions import it; they never call `axios.create()` or `fetch` themselves. The client is the single transport adapter — the FE counterpart to the backend's thin endpoints (see backend `minimal-api-endpoints.md`): attach auth, forward cancellation, unwrap the success envelope, normalise errors. **No business logic in the client.**

## Instance
```ts
// lib/api/client.ts
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,   // never hardcode hosts
  timeout: 15_000,
});

// 1. attach the in-memory JWT bearer to every request
apiClient.interceptors.request.use((config) => {
  const token = authStore.getToken();      // in-memory only (see auth-flow.md)
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 2. normalise non-2xx into typed ProblemDetails; handle 401 globally
apiClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) authStore.clearAndRedirect(); // no refresh in MVP
    return Promise.reject(parseProblemDetails(error)); // see error-handling.md
  },
);
```

## The `ApiResponse<T>` success envelope
The backend wraps every success payload in `{ success, data }` (backend `minimal-api-endpoints.md`). Model it **once** and unwrap it so features receive the inner payload `T` — never the envelope.

```ts
// lib/api/apiResponse.ts
export interface ApiResponse<T> { success: boolean; data: T; }

// unwrap an Axios response whose body is ApiResponse<T> -> T
export const unwrap = <T>(res: AxiosResponse<ApiResponse<T>>): T => res.data.data;
```

## Feature request functions
Request functions are thin: call the client, forward the `AbortSignal`, unwrap, return the typed payload.
```ts
// features/matches/api/getMatchCalendar.ts
export const getMatchCalendar = (signal?: AbortSignal): Promise<MatchCalendarResponse> =>
  apiClient
    .get<ApiResponse<MatchCalendarResponse>>("/api/matches", { signal })
    .then(unwrap);
```
Lists arrive as `ApiResponse<T[]>`; `unwrap` returns the `T[]`.

## Interceptor contract (keep intact)
1. **Attach JWT** bearer (in-memory token; see `auth-flow.md`).
2. **Forward the `AbortSignal`** from TanStack Query per request (`{ signal }`) so unmount/navigation cancels in-flight calls (see `server-state-tanstack-query.md`).
3. **Unwrap** the `ApiResponse<T>` envelope so callers get `T`.
4. **Parse non-2xx** into a typed `ProblemDetails` before it reaches hooks (see `error-handling.md`).
5. **Handle `401` globally** — clear the in-memory session and redirect to login. No silent token refresh in the MVP.

## Rules
- **Single instance** — import `apiClient`; never construct another or use bare `fetch` in a feature.
- **Base URL from env** — `import.meta.env.VITE_API_URL`; never hardcode hosts. Default timeout on the instance.
- Request functions declare the **inner** response DTO type and return it (envelope unwrapped) — see `types-dtos.md`.
- The client never builds UI, never toasts, never knows about React — it rejects with `ProblemDetails` and lets hooks/components decide (see `error-handling.md`).
- Strongly-typed ids from the backend arrive as their raw value; keep response types in sync with the contract (regenerate from Swagger/OpenAPI where possible).
- The correlation id (`X-Correlation-Id`) is accepted/returned by the backend; if the app sets one, send it as that header so a request can be traced end to end (backend `observability.md`).
