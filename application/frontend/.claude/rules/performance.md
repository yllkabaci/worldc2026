---
paths:
  - "src/**/*.tsx"
  - "src/**/*.ts"
---

# Performance

Optimise for **measured** problems, not assumptions. The default React render is fast enough for almost every screen in this app; premature memoization adds noise and bugs.

## Memoization
- Apply `useMemo` / `useCallback` / `React.memo` **only where a measured re-render problem exists** (e.g. a large leaderboard list), and add a brief comment saying why.
- Do **not** wrap every value/handler by reflex. Cargo-cult memoization is flagged in review.
- Stable identities matter only when they feed a memoized child or an effect dependency — otherwise skip them.

## Lists & data
- **Paginate or virtualise** large lists (full leaderboard) rather than rendering thousands of rows.
- Let TanStack Query handle caching/dedup; tune `staleTime` per resource instead of caching by hand (see `server-state-tanstack-query.md`).
- Render the values the API returns — don't recompute scores/ranks client-side (see `client-vs-server-responsibility.md`).

## Bundle & dependencies
- **No new heavy dependency** for something the stack already covers (data fetching, forms, validation, routing, i18n). Reuse TanStack Query, React Hook Form, Zod, React Router, react-i18next.
- Lazy-load heavy or rarely-visited route subtrees with `React.lazy` + `Suspense` where it meaningfully cuts the initial bundle.
- Keep `src/components` lean and tree-shakeable; avoid barrel files that pull in unrelated modules.

## Measure first
- Before optimising, confirm the cost with the React Profiler or a real measurement. A change justified as "for performance" should reference what was observed.
