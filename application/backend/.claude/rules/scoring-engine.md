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

## Algorithm (fixed)
Points are awarded for exactly one of two outcomes (configurable values from the rule set, defaults shown):
1. **Exact score** - both predicted goals match the result: `3` points.
2. **Correct winner or draw** (but not the exact score) - the predicted outcome (home win / away win / draw) matches: `1` point.
3. Otherwise (a miss): `0` points.

There are **no bonus predictions and no stage multipliers** - these have been removed from the product. The engine takes the prediction, the official result, and the rule set's two point values, and returns the single awarded outcome.

## Points type & bounds
- Points are **`decimal`** - never `double`/`float` (no rounding is applied).
- Points are **never negative**; clamp the result at `0` (BR-002).
- Goals are **integers**.

## Void & special cases (encode explicitly)
- **Cancelled match**: all predictions are **void** - `0` points, no penalty (BR-008).
- Scoring uses the **official `MatchResult` recorded by the admin** (UC-A10).

## Transparency
- Return an itemized `PointsBreakdown` value object (the awarded outcome and its points) so results are auditable and explainable - not just a single number.

## Testing
- The scoring engine gets **exhaustive unit tests**: exact score, correct winner, correct draw, miss, the cancelled-match void case, the clamp-at-zero rule, and decimal precision. It is the most heavily tested unit in the codebase (see `testing-conventions.md`).
