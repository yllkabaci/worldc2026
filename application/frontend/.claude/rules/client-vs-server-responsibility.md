---
paths:
  - "src/features/**/*"
  - "src/lib/**/*"
---

# Client vs Server Responsibility

The backend is the **system of record** and owns all business truth (backend `business-rule-placement.md`). The SPA holds **no scoring or business logic** — it renders what the API returns and submits intent. All correctness lives server-side; the UI mirrors validation only for fast feedback.

## Placement framework

| Concern | Where it belongs |
|---------|------------------|
| Scoring, points arithmetic, void rules | **Backend domain** — never recomputed on the client |
| Business invariants (deadline passed, one prediction per match, match settleable) | **Backend domain**; the UI shows server rejection (see `error-handling.md`) |
| Input shape feedback (types, ranges, required) | **Client** Zod schema, mirroring backend bounds (see `forms-validation.md`) |
| Orchestrating load → render → submit | **Client** feature hooks + components |
| Display formatting (dates, locale, ordinal ranks) | **Client** — presentation only, no domain decisions |
| Auth/session, UI toggles | **Client** state |
| Matches, predictions, leaderboards | **Server state** via TanStack Query (see `server-state-tanstack-query.md`) |

## The line between client validation and server truth
- **Client (allowed):** instant feedback on shape — "goals must be 0–20", "field required", "must be a number".
- **Server (authoritative):** "prediction window closed" (BR-007), "duplicate prediction", scoring, leaderboard order, rule versioning. The client **never** decides these; it disables UI as a hint and trusts the server's response as final.

## Rules
- **Never replicate scoring or leaderboard math on the client.** Render the points/ranks the API returns.
- **Never gate a business action on client-only checks.** Disabling a submit button after a visible deadline is UX; the server still validates and may reject — handle that rejection (see `error-handling.md`).
- **Deadline / lock display** is derived from server data (e.g. `match.lockAt`), shown for feedback — the server enforces it.
- When in doubt: if the rule is a *product decision about the domain*, it lives server-side and the client only reflects the outcome.
