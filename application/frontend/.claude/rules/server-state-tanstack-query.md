---
paths:
  - "src/features/**/api/**/*.ts"
  - "src/features/**/api/**/*.tsx"
  - "src/lib/api/queryKeys.ts"
  - "src/app/queryClient.ts"
---

# Server State with TanStack Query

All server state lives in the TanStack Query cache — never duplicated into `useState`. **Queries** read state; **mutations** change it. This is the frontend counterpart to the backend's CQRS split (see backend `mediatr-cqrs.md`): one hook per use case, named after it.

## Queries (reads)
```ts
// features/matches/api/useMatchCalendar.ts
export function useMatchCalendar() {
  return useQuery({
    queryKey: queryKeys.matches.calendar(),     // from the factory - never inline strings
    queryFn: ({ signal }) => getMatchCalendar(signal),  // signal -> Axios (see api-client-axios.md)
    staleTime: 60_000,                           // calendar changes slowly
  });
}
```

## Mutations (writes)
```ts
// features/predictions/api/useMakePrediction.ts
export function useMakePrediction() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: MakePredictionRequest) => postPrediction(cmd),
    onSuccess: () => {
      // settling/predicting affects both boards - invalidate every affected key
      qc.invalidateQueries({ queryKey: queryKeys.predictions.all() });
      qc.invalidateQueries({ queryKey: queryKeys.leaderboard.all() });
    },
  });
}
```

## Query Key Factory
- All keys come from `lib/api/queryKeys.ts` — **never** inline string arrays.
```ts
export const queryKeys = {
  matches:      { all: () => ["matches"] as const,
                  calendar: () => ["matches", "calendar"] as const,
                  detail: (id: string) => ["matches", "detail", id] as const },
  predictions:  { all: () => ["predictions"] as const,
                  mine: () => ["predictions", "mine"] as const },
  leaderboard:  { all: () => ["leaderboard"] as const },
} as const;
```

## Rules
- **One hook per use case** in `features/<x>/api`; no fat hooks spanning multiple use cases.
- Reads use `useQuery`; writes use `useMutation`. Never mutate via a query.
- **Invalidate** affected keys in `onSuccess` so the UI re-syncs (predicting/settling → `predictions` + `leaderboard`).
- **No `useEffect` fetching** — TanStack Query owns the lifecycle (see `components-presentation-only.md`).
- Forward the query `AbortSignal` into the Axios call so unmount/navigation cancels the request — the FE counterpart to the backend `CancellationToken`.
- **Defaults** in `app/queryClient.ts`: per-resource `staleTime` (calendar longer, leaderboard shorter), **retry off for `4xx`**.
- Server data is **never** copied into local `useState`; derive from `data` or keep a hook. Only UI toggles and session are client state.
