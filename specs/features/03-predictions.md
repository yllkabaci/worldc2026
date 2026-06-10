# 03 — Predictions (spine)

**Slices:** `MakePrediction` (command), `ModifyPrediction` (command), `GetMyActivePredictions` (query)
**Feature folder:** `Features/Predictions` · **Auth:** User
**Consumes:** SPEC §6, business doc §4.3 + BR-004–010, `.claude/rules/*`.

## System Overview
Lets an authenticated user predict the regulation score of a match before its deadline, change it while the window is open, and list their pending predictions. The core use case of the app.

## Behavioral Contract
- When a user submits a prediction (home/away goals) for a match that is **open** (teams known, before the deadline), the system stores it as that user's single prediction for the match, timestamped, and confirms.
- When a user **creates** (`POST`) a prediction for a match they have already predicted, the system rejects with `409` (one active prediction per match, BR-005). To change it, the user **modifies** (`PUT`) the prediction, which replaces it while the window is open (last write wins, unlimited edits, BR-006).
- When a user predicts after the deadline, the system rejects with `409` and changes nothing (BR-007) — even if the match is delayed.
- When a user predicts a match whose teams are undetermined (knockout slot not resolved), the system rejects; once teams are set and it is before the deadline, the same submission succeeds.
- When a user requests their active predictions, the system returns those awaiting results.

## Explicit Non-Behaviors
- Must **not** accept or modify a prediction at or after the deadline.
- Must **not** allow more than one active prediction per match per user (BR-005).
- Must **not** let a blocked user predict (BR-009).
- Must **not** disclose another user's prediction for a match before that match locks.
- Must **not** award points — scoring happens at settlement (feature 04).

## Integration Boundaries
- Persistence via `IApplicationDbContext`; current time via `IClock`; identity via `ICurrentUserService`.

## Domain Notes
- `Prediction` aggregate (**own root**): `MatchId`, `UserId`, `Score(home,away)`, optional `BonusPrediction` (tier 2). `Place(...)` / `Revise(...)` throw `PredictionWindowClosedException` if `now ≥ deadline`. Deadline read from the `Match`.

## Validation (input shape only)
- Goals are integers `0–20` (BR-010). Bonus-prediction inputs (cards/subs/minute/etc.) are tier 2; validate their ranges when added (BR-024–028).

## Error Codes
`MatchNotFound` (404), `PredictionWindowClosed` (409), `DuplicatePrediction` (409, only if not treated as an edit), `Forbidden`/blocked (403), `ValidationError` (400).

## Definition of Done
Make/modify obey the deadline and one-per-match rule; last write wins before lock; pending list is correct; others' predictions are hidden before lock. Verified by holdout scenarios (external).

## Resolved Decisions
1. **Exact-score only** in the MVP; bonus predictions are tier 2.
2. **Distinct verbs**: `POST` create (a duplicate for the same match → `409`) and `PUT` modify (replace while open).
