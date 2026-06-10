Read the business rules in `SPEC.md`, the per-feature specs in `specs/features/`, and `WorldCup2026 BusinessLogic EN.docx`. Then scan the backend codebase under `application/backend/src` for rules BR-$ARGUMENTS.

For each rule, report exactly one of:
- IMPLEMENTED — file + how (cite the domain method / handler / validator)
- MISSING — not found
- INCORRECT — found but logic diverges from the spec

Remember our placement rules: business invariants live in the **domain** (aggregates / ScoringService), not in endpoints or validators; validators check input shape only. Scoring is base-only for the MVP (bonuses/multipliers are tier 2).

Output a table with file paths and line numbers. Be specific. Do not fix anything — report only.
