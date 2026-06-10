---
paths:
  - "src/lib/api/**/*.ts"
---

# API Client (Centralised Axios)

There is **one** Axios instance, in `lib/api/client.ts`. Feature `api/` functions import it; they never call `axios.create()` or `fetch` themselves. The client is the single transport adapter — the FE counterpart to the backend's thin endpoints (see backend `minimal-api-endpoints.md`): attach auth, forward cancellation, normalise errors. **No business logic in the client.**

## Structure
```ts
// lib/api/client.ts
export const apiClient = axios.create({ baseURL: import.meta.env.VITE_API_URL });

// 1. attach JWT bearer to every request
apiClient.interceptors.request.use((config) => {
  const token = authStore.getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 2. parse non-2xx into typed ProblemDetails before it reaches hooks
apiClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) authStore.clearAndRedirect(); // 3. global 401
    return Promise.reject(parseProblemDetails(error)); // see error-handling.md
  },
);
```

## Interceptor contract (keep intact)
1. **Attach JWT** bearer token to every request.
2. **Forward the `AbortSignal`** from TanStack Query so unmount/navigation cancels in-flight requests (see `server-state-tanstack-query.md`).
3. **Parse non-2xx** responses into a typed `ProblemDetails` before they reach hooks (see `error-handling.md`).
4. **Handle `401` globally** — clear session, redirect to login.

## Feature request functions
```ts
// features/matches/api/getMatchCalendar.ts
export const getMatchCalendar = (signal?: AbortSignal) =>
  apiClient.get<MatchCalendarResponse>("/api/matches", { signal }).then((r) => r.data);
```

## Rules
- **Single instance** — import `apiClient`; never construct a new one or use bare `fetch` in a feature.
- **Base URL from env** — `import.meta.env.VITE_API_URL`; never hardcode hosts.
- The client returns **typed** data; request functions declare the response DTO type (see `types-dtos.md`).
- The client never builds UI, never toasts, never knows about React — it rejects with `ProblemDetails` and lets hooks/components decide (see `error-handling.md`).
- Strongly-typed ids from the backend arrive as their raw value; keep the response types in sync with the backend contract.
