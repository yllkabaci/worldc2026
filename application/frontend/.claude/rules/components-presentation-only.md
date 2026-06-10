---
paths:
  - "src/**/components/**/*.tsx"
  - "src/components/**/*.tsx"
---

# Components Must Be Presentation Only

Components render UI and raise events. They must **NEVER** call Axios, `fetch`, or `lib/api/client` directly, and must **NEVER** fetch data in `useEffect`. Data access and mutations live in the feature's `api/` hooks (TanStack Query); cross-cutting feature logic lives in `hooks/`. This keeps a component swappable, testable, and free of transport concerns — the screen is the only thing that composes data hooks with presentation.

## Current data & actions

A component receives server data and mutation triggers from a **feature hook**, not by fetching itself.

```tsx
// Bad - component coupled to the transport layer
function MatchCalendar() {
  const [matches, setMatches] = useState<Match[]>([]);
  useEffect(() => {
    axios.get("/api/matches").then((r) => setMatches(r.data)); // NO
  }, []);
  return <ul>{matches.map(/* ... */)}</ul>;
}

// Good - data comes from a TanStack Query hook in features/matches/api
function MatchCalendar() {
  const { data, isLoading, isError } = useMatchCalendar();
  if (isLoading) return <CalendarSkeleton />;
  if (isError) return <ErrorState />;
  if (!data?.length) return <EmptyState message="No upcoming matches" />;
  return <ul>{data.map((m) => <MatchRow key={m.id} match={m} />)}</ul>;
}
```

## Rules
- **Functional components only**, with an explicit `interface Props {}`. No class components except the Error Boundary (see `error-handling.md`).
- **No HTTP in components** — no `axios`, no `fetch`, no `client`. Use `useQuery`/`useMutation` hooks from `features/<x>/api` (see `server-state-tanstack-query.md`).
- **No `useEffect` data fetching.** `useEffect` is for genuine side effects (subscriptions, focus), not loading data.
- **No business/scoring logic** in a component (see `client-vs-server-responsibility.md`).
- **Always render loading / empty / error branches** for any data-backed view; never a bare blank screen.
- Keep components **dumb about routing/auth** — gating happens at the route level (see `routing-auth.md`).

## Summary

| Need | Solution |
|------|----------|
| Read server data | `useQuery` hook in `features/<x>/api` |
| Change server state | `useMutation` hook in `features/<x>/api` |
| Feature logic (derive, format, orchestrate) | hook in `features/<x>/hooks` |
| Current user / session | `useAuth` from `lib/auth` |
| Translated copy | `useTranslation` (see `i18n-a11y.md`) |
