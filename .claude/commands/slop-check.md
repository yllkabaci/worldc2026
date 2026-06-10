Audit the codebase for AI slop and rule violations. Check against `application/backend/.claude/rules` and `application/frontend/.claude/rules`.

Backend:
1. MVC controllers, a `Result<T>`/Outcome type, or `double`/`float` used for points (all forbidden).
2. Business logic in endpoints/handlers that belongs in the domain; endpoints that aren't transport-only.
3. Handlers depending on `HttpContext` instead of `ICurrentUserService`; handlers calling `SaveChanges` directly.
4. Missing `ApiResponse<T>` envelope on success; failures not going through RFC 7807.
5. Raw SQL; missing `cancellationToken` propagation.

Frontend:
6. `any` types; `console.log`; inline styles; Axios calls inside components; `useEffect` data fetching.
7. Hardcoded match/team/player data; missing ARIA labels; layout broken at 375px; missing skeleton states.

General: placeholder text ("Lorem", "Team A", "TODO", "FIXME", "Coming soon").

Report each finding: file, line, description, severity (High/Medium/Low). Fix all High-severity findings immediately.
