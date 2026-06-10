Run the full review suite on the current change set and return one consolidated report.

1. **Scope:** `git diff --merge-base origin/main --name-only` (fall back to staged changes, then `git diff HEAD~1`). List the files in scope. If empty, stop and say so.
2. **Dispatch in parallel** — launch all five reviewers at once (single batch of Task calls) on that scope: `code-reviewer`, `architecture-reviewer`, `test-reviewer`, `business-rules-reviewer`, `security-reviewer`.
3. **Consolidate:** merge findings into one list, de-duplicate co-findings (note corroboration), sort HIGH → MEDIUM → LOW.
4. **Verdict:** any HIGH → BLOCK; MEDIUM only → APPROVE-WITH-NITS; LOW only → APPROVE. Map to the three quality gates (SPEC §8).

Report only — do not fix anything. Each finding: `file:line`, what, why, fix, and which reviewer raised it.
