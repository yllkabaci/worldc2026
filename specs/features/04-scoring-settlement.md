# 04 — Scoring & Settlement (spine, correctness core)

**Slices:** `SettleMatch` (internal command, triggered by `SetOfficialResult`)
**Feature folder:** `Features/Scoring` · **Auth:** system (not a public endpoint)
**Consumes:** SPEC §6, business doc §6.1 + BR-001/002/008/026/027, **`.claude/rules/scoring-engine.md`**.

## System Overview
When a match's official result is recorded, settlement scores every prediction on that match using the pure scoring engine and the rule set pinned to the match, then adds the points to each user's total. This is the fairness-critical core.

## Behavioral Contract
- When a match is settled with its official result, the system scores **every** prediction on that match deterministically and credits each user's total with the resulting points.
- When the same match settlement is received again, the system makes **no** further change to any total (idempotent — score once).
- When a match is **cancelled**, all its predictions are **void**: 0 points, no penalty (BR-008).
- Scoring is a **pure function** of `(prediction, official result, pinned ScoringRuleSet)` — same inputs always produce the same `PointsBreakdown`.

## Scoring rules (per `scoring-engine.md`)
- Base (configurable defaults): exact score `3`, correct winner `1`, correct draw `1`, miss `0`.
- **MVP = base only.** Bonuses **and** stage multipliers are tier 2; when added, the multiplier applies to the match total (BR-001). The engine is structured to support them without rework.
- Points are **`decimal`**, **never negative** (clamp at 0, BR-002), **no rounding**.
- Void cases: minute-of-first-goal void on `0–0` (BR-026); penalty bonus excludes shootouts (BR-027).
- A match is scored by the rule set effective **as of its kickoff**. The MVP uses a **single mutable `ScoringRuleSet`**; the match records the rule-set id it was scored with (effective-dated history is tier 2).

## Explicit Non-Behaviors
- Must **not** award points before a match is settled.
- Must **not** award points more than once per match per user.
- Must **not** use `double`/`float` for points, nor apply extra-time/shootout scores to the regulation-based scoring.
- Must **not** perform any I/O inside the scoring function (no DB/clock/HTTP/random).

## Integration Boundaries
- Triggered by feature 02 (`SetOfficialResult`) via a domain event / internal command; persistence via `IApplicationDbContext`.

## Domain Notes
- `ScoringService` (pure, `Domain/Scoring`) → `PointsBreakdown` (itemized). `ScoringRuleSet` versioned/immutable. User total updated from awarded points (feeds feature 05).

## Error Codes
`MatchNotSettleable` (409) when settling a match without a valid recorded result.

## Definition of Done
Settling a match credits correct, deterministic, decimal totals; re-settlement is a no-op; void cases yield zero. The scoring engine is covered **exhaustively** by unit tests and by external holdout scenarios (predict → settle → assert totals).

## Resolved Decisions
1. **Base scoring only** in the MVP (exact/winner/draw); bonuses tier 2.
2. **Stage multipliers tier 2** (engine supports them).
3. **Single mutable `ScoringRuleSet`**; the match records the rule-set id used; effective-dated history tier 2.
4. User total is a **stored running total** updated by the settlement event (and feeds the exact-hit count used for the leaderboard tiebreak).
5. Knockouts scored on the **regulation 90-minute** result.
