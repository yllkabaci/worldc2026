# 02 — Matches (spine)

**Slices:** `GetMatchCalendar` (query), `GetMatchDetails` (query), `SetOfficialResult` (command), `CancelOrPostponeMatch` (command), `ResettleMatch` (command, audited)
**Feature folder:** `Features/Matches` · **Auth:** calendar/details anon or User; `SetOfficialResult` Admin
**Consumes:** SPEC §6, business doc §4.5/§5.4, `scoring-engine.md`, `.claude/rules/*`.

## System Overview
Exposes the tournament fixtures to everyone and lets an admin record an official result, which triggers settlement (feature 04). Matches originate from the Fixture/Result provider (twin in non-prod); admins can override.

## Behavioral Contract
- When anyone requests the calendar, the system returns matches with stage, teams, kickoff (UTC), status, and (if known) result; filterable by stage/date/status.
- When anyone requests a match by id, the system returns its details, or `404` if it does not exist.
- When an **admin** sets the official result of a match that is `Finished` (or eligible), the system records the regulation result (home/away goals, plus bonus fields when present), marks the match `Settled`, and triggers scoring (feature 04).
- When a **non-admin** attempts to set a result, the system returns `403`.
- When an admin sets a result for a non-existent match, the system returns `404`; for a match not in a settleable state, `409`.
- When an **admin cancels** a match, the system marks it `Cancelled`; its predictions become **void** at settlement (0 points, no penalty, BR-008).
- When an **admin postpones** a match to a new kickoff, the system updates the kickoff and **recomputes the prediction deadline** (= new kickoff − window); existing predictions are kept (UC-A11).
- When an **admin re-records** the result of an already-**settled** match, the system requires a **second confirmation**, writes an **immutable audit entry**, and **re-runs settlement** to correct totals (UC-A10).

## Explicit Non-Behaviors
- Must **not** allow non-admins to set/cancel/postpone matches.
- Must **not** award any points itself — settlement/scoring is feature 04.
- Must **not** invent fixtures or results; data comes from the provider or the admin.
- Must **not** apply extra-time/shootout scores to the recorded regulation result used for scoring.

## Integration Boundaries
- `IFootballApi` supplies fixtures/results. Provider = **football-data.org** (v4, `X-Auth-Token`, competition `WC`); a deterministic **twin** is used in dev/test and when no API token is set. Admin entry overrides imported data (UC-A02/§8.3).
- Persistence via `IApplicationDbContext`.

## Domain Notes
- `Match` aggregate (root): stage, two teams (nullable until a knockout slot resolves), kickoff, `PredictionDeadline` (= kickoff − window), status (`Upcoming → Live → Finished/Cancelled`), official `Result`, and the **`ScoringRuleSetId` pinned as of kickoff**. Methods: `Schedule`, `OpenForPredictions`, `Settle`, `Cancel`, `Postpone`.

## Validation (input shape only)
- `SetOfficialResult`: goals are integers `0–20`; bonus fields within their ranges when supplied (cards `0–20`/`0–10`, subs `0–5`).

## Error Codes
`MatchNotFound` (404), `MatchNotSettleable` (409), `Forbidden` (403), `ValidationError` (400).

## Definition of Done
Calendar/details return correct data; an admin result transitions the match and triggers settlement; authz blocks non-admins. Verified by holdout scenarios (external).

## Resolved Decisions
1. **Cancel/postpone are in the MVP** (UC-A11): cancel → predictions void; postpone → deadline resets to the new kickoff.
2. **Re-settlement is in the MVP** (UC-A10): changing a settled result requires double-confirmation + an immutable audit entry, then re-runs settlement.
3. Knockouts are scored on the **regulation 90-minute** result (extra time/shootouts ignored).
4. Fixture/result source = **football-data.org** (twin in dev/test). **Verify** how the provider reports the 90-minute score for matches that go to extra time before relying on `score.fullTime` for the regulation result.
