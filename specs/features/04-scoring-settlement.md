# 04 — Scoring & Settlement (spine, correctness core)

**Slices:** `SettleMatch` (internal command, triggered by `SetOfficialResult`)
**Feature folder:** `Features/Scoring` · **Auth:** system (not a public endpoint)
**Consumes:** SPEC §6, business doc §6.1 + BR-002/008, **`.claude/rules/scoring-engine.md`**.

## System Overview
When a match's official result is recorded, settlement scores every prediction on that match using the pure scoring engine and the rule set pinned to the match, then adds the points to each user's total. This is the fairness-critical core.

## Behavioral Contract
- When a match is settled with its official result, the system scores **every** prediction on that match deterministically and credits each user's total with the resulting points.
- When the same match settlement is received again, the system makes **no** further change to any total (idempotent — score once).
- When a match is **cancelled**, all its predictions are **void**: 0 points, no penalty (BR-008).
- Scoring is a **pure function** of `(prediction, official result, pinned ScoringRuleSet)` — same inputs always produce the same `PointsBreakdown`.

## Scoring rules (per `scoring-engine.md`)
- Points are awarded for exactly two outcomes (configurable defaults): **exact score `3`**, **correct winner or draw `1`**, otherwise **`0`**.
- There are **no bonus predictions and no stage multipliers** — these have been removed from the product.
- Points are **`decimal`**, **never negative** (clamp at 0, BR-002), **no rounding**.
- A match is scored by the rule set effective **as of its kickoff**. The MVP uses a **single mutable `ScoringRuleSet`**; the match records the rule-set id it was scored with (effective-dated history is tier 2).

## Explicit Non-Behaviors
- Must **not** award points before a match is settled.
- Must **not** award points more than once per match per user.
- Must **not** use `double`/`float` for points, nor apply extra-time/shootout scores to the regulation-based scoring.
- Must **not** award any points beyond the exact-score and correct-winner/draw outcomes (no bonuses, no multipliers).
- Must **not** perform any I/O inside the scoring function (no DB/clock/HTTP/random).

## Integration Boundaries
- Triggered by feature 02 (`SetOfficialResult`) via a domain event / internal command; persistence via `IApplicationDbContext`.

## Domain Notes
- `ScoringService` (pure, `Domain/Scoring`) → `PointsBreakdown` (the awarded outcome and its points). `ScoringRuleSet` versioned/immutable, holding only the two point values. User total updated from awarded points (feeds feature 05).

## Error Codes
`MatchNotSettleable` (409) when settling a match without a valid recorded result.

## Definition of Done
Settling a match credits correct, deterministic, decimal totals; re-settlement is a no-op; void cases yield zero. The scoring engine is covered **exhaustively** by unit tests and by external holdout scenarios (predict → settle → assert totals).

## Resolved Decisions
1. Scoring awards points **only** for an exact score (`3`) and a correct winner/draw (`1`); a miss is `0`. **No bonuses, no stage multipliers** (removed from the product).
2. **Single mutable `ScoringRuleSet`**; the match records the rule-set id used; effective-dated history tier 2.
3. User total is a **stored running total** updated by the settlement event (and feeds the exact-hit count used for the leaderboard tiebreak).
4. Knockouts scored on the **regulation 90-minute** result.
