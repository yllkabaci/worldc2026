# 05 — Leaderboard (spine)

**Slices:** `GetGlobalLeaderboard` (query), `GetMyRanking` (query)
**Feature folder:** `Features/Leaderboard` · **Auth:** User
**Consumes:** SPEC §6, business doc §4.4 + BR-003, `.claude/rules/*`.

## System Overview
Ranks users by total points and shows a user their own position. Reflects points awarded at settlement (feature 04); holds no scoring logic of its own.

## Behavioral Contract
- When a user requests the global leaderboard, the system returns users ranked by **total points, descending**, each appearing exactly once, with rank, display name, and points.
- Ties are broken by **exact-score accuracy** (count of exact-score predictions, BR-003); a deterministic secondary (earliest registration) guarantees a total, stable order at the top.
- When a user requests their ranking, the system returns their current position and points.
- The ordering is deterministic and stable across repeated reads given the same data.
- When a user applies **period (weekly/monthly/all-time), stage, or country filters**, the system returns the ranking computed over the matching subset (UC-U09).

## Explicit Non-Behaviors
- Must **not** compute or re-derive scoring — it reads totals produced by settlement.
- Must **not** show duplicate user rows or non-deterministic ordering.
- Must **not** include blocked users' standings if blocking removes them from the leaderboard (UC-A08) — *tier 2; see ambiguity*.

## Integration Boundaries
- Read-only over `IApplicationDbContext` with `AsNoTracking()`, projected straight to the response DTO (per `mediatr-cqrs.md`).

## Domain Notes
- Reads from stored user totals + an exact-hit count maintained at settlement (feature 04). No mutation here.

## Validation (input shape only)
- Filter inputs (period/stage/country) and paging are validated for shape only. The board supports these filters in the MVP (UC-U09).

## Error Codes
None specific beyond standard auth (`401`).

## Definition of Done
Ranking is correct, unique-per-user, and deterministic incl. the tiebreak; my-ranking matches the global position. Verified by holdout scenarios (external) — e.g. settle a fixed set of predictions, assert the exact ordering and a deterministic tiebreak winner.

## Resolved Decisions
1. Tiebreak: exact-score accuracy (BR-003), then **earliest registration** → unique winner.
2. **Filters are in the MVP**: period (weekly/monthly/all-time), stage, country.
3. Blocked-user removal from the leaderboard (UC-A08) stays **tier 2**.
