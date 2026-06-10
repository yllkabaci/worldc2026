---
paths:
  - "src/**/Domain/Scoring/**/*.cs"
---

# Scoring Engine (Correctness Core)

Scoring is the fairness-critical heart of the system. It MUST be a **pure, deterministic function** so it can be exhaustively unit-tested and verified by external holdout scenarios without reading code.

## Purity & determinism
- Signature (conceptually): `PointsBreakdown Score(Prediction prediction, MatchResult result, ScoringRuleSet ruleSet)`.
- **No I/O**: no database, no `HttpClient`, no `IClock`, no `DateTime.UtcNow`, no randomness. Everything needed is passed in.
- Same inputs -> same `PointsBreakdown`, every time. The function is referentially transparent.
- Lives in `Domain/Scoring` (a domain service / static calculator); has no infrastructure dependencies (see `domain-model-ddd.md`).

## Rule versioning (effective-dated)
- `ScoringRuleSet` is **immutable once published** and carries an effective timestamp.
- A match is scored by the rule set **effective as of its kickoff**; a config change applies only to matches kicking off after it (business doc §8.2). The resolved `ScoringRuleSetId` is pinned onto the `Match` no later than when it opens for predictions, and settlement uses that pinned version.
- Never mutate a published rule set; publish a new version.

## Algorithm (fixed order)
1. **Base points** by outcome (configurable, defaults): exact score `3`, correct winner `1`, correct draw `1`, otherwise `0`.
2. **Enabled bonuses**, each added if correct: exact goal-difference, first goalscorer, team to kick off, yellow-cards total, red-cards total, minute of first goal (±tolerance), penalty yes/no, substitutions per team. Each bonus's points and enabled-flag come from the rule set.
3. **Sum** base + bonuses = the match total.
4. **Stage multiplier** is applied to the **match total**, not to individual bonuses (BR-001).

## Points type & bounds
- Points are **`decimal`** - never `double`/`float` (multipliers like `1.5x`/`2.0x` produce fractions that must stay exact; no rounding is applied).
- Points are **never negative**; clamp the result at `0` (BR-002).
- Goals, cards, and substitution counts are **integers**.

## Void & special cases (encode explicitly)
- **Minute of first goal**: if the match ends `0-0`, this bonus is **void** - no points, no penalty (BR-026); apply the configured ±tolerance otherwise.
- **Cancelled match**: all predictions are **void** - `0` points, no penalty (BR-008).
- **Penalty bonus**: counts penalties awarded **during** the match; penalty **shootouts** are excluded (BR-027).
- Scoring uses the **official `MatchResult` recorded by the admin** (UC-A10).

## Transparency
- Return an itemized `PointsBreakdown` value object (base, each bonus contribution, multiplier, final total) so results are auditable and explainable - not just a single number.

## Testing
- The scoring engine gets **exhaustive unit tests**: every tier, every bonus, void cases, multiplier math, the clamp-at-zero rule, and decimal precision. It is the most heavily tested unit in the codebase (see `testing-conventions.md`).
