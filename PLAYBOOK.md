# PLAYBOOK.md — Claude Code Hackathon Playbook

> How the team uses Claude Code's advanced features to operate as a system. Read before the hackathon.
> Reconciled to this repo's stack: .NET 10 Minimal API + MediatR, React + Vite, holdout kept **outside** the repo.

---

## 1. Hooks
Hooks are shell commands Claude Code runs **automatically** on events (before/after tool use, on prompt submit, on stop). Set once, they run silently for every agent. They live in `.claude/settings.json` at the repo root (or run `/update-config` and describe them in plain English).

### Hook 1 — Slop audit on agent Stop
Prints a checklist so no one ships incomplete work. **Event:** `Stop`
```json
{ "hooks": { "Stop": [{ "hooks": [{ "type": "command",
  "command": "echo '--- SLOP AUDIT ---\n1. Endpoints transport-only (no logic/data access)?\n2. Business logic in the domain, not handlers/endpoints?\n3. No MVC controllers / no Result<T> / no double|float for points?\n4. Typed exceptions -> RFC 7807 (WC-NNNN)?\n5. ApiResponse<T> envelope on success?\n6. ARIA labels + 375px layout (frontend)?\n7. No hardcoded match/team/player data, no any, no TODO?'" }] }] } }
```

### Hook 2 — Tests on every Edit
**Event:** `PostToolUse` matcher `Edit`
```json
{ "hooks": { "PostToolUse": [{ "matcher": "Edit", "hooks": [{ "type": "command",
  "command": "cd application/backend && dotnet test --no-build --verbosity quiet 2>&1 | tail -5" }] }] } }
```

### Hook 3 — Spec guard on Write
**Event:** `PreToolUse` matcher `Write`
```json
{ "hooks": { "PreToolUse": [{ "matcher": "Write", "hooks": [{ "type": "command",
  "command": "echo 'PRE-WRITE: align with SPEC.md + the feature spec in specs/features. Honor application/backend/.claude/rules (Minimal API, MediatR, no controllers).'" }] }] } }
```

### Hook 4 — Inject spec context on every prompt
**Event:** `UserPromptSubmit`
```json
{ "hooks": { "UserPromptSubmit": [{ "hooks": [{ "type": "command",
  "command": "echo 'CONTEXT: read SPEC.md, the relevant specs/features/*.md, and .claude/rules before implementing. Halt on ambiguity.'" }] }] } }
```

**Why:** four automated checkpoints across every workstream catch errors immediately instead of at the 2:15 audit.

---

## 2. Parallel sub-agents
One person runs multiple independent Claude Code tasks at once. Five people × ~3 agents ≈ 15 workstreams. Build phase is 90 min — sequential doesn't fit; parallel completes a batch in the time of the slowest task.

### Build-phase plan (0:45)
Drive each slice through the **`create-feature` skill** with its spec, then **`write-unit-tests`**, then the review agents. Independent slices fan out in parallel.

**Backend — 3 parallel agents:**
```
Agent 1: "Use create-feature with specs/features/01-auth.md. Build the Auth slice
(Register, Login) per .claude/rules: Minimal API endpoints, MediatR ICommand/handlers,
domain User aggregate, JWT issuance. Then write-unit-tests."

Agent 2: "Use create-feature with specs/features/04-scoring-settlement.md. Build the pure
ScoringService in Domain/Scoring (base points, decimal, void cases, never negative) and
SettleMatch. Cover it exhaustively with write-unit-tests (Theory/InlineData)."

Agent 3: "Use create-feature with specs/features/02-matches.md. Build Matches
(calendar, details, admin set-result, cancel/postpone, audited re-settlement). Then write-unit-tests."
```
**Frontend — 3 parallel agents** (feature folders mirroring backend slices, TanStack Query, RHF+Zod, ARIA, 375px): auth screens; matches calendar + prediction slip; leaderboard + my-ranking.

Run the review agents (architecture, code, business-rules/scoring, security, test) on each result before marking done.

---

## 3. Custom slash commands
Reusable team prompts in `.claude/commands/*.md`, invoked as `/name` (use `$ARGUMENTS` for input). Ours (see that folder): `/check-br`, `/seed-demo`, `/slop-check`, `/verify-scenario`, `/demo-script`. Shared commands = shared vocabulary = consistent output across five sessions.

---

## 4. Routines (scheduled background agents)
- **Match data syncer** — every 5 min, refresh fixtures/results from the football API into the seed (`application/backend/src/WorldCup.Infrastructure/Persistence/SeedData.cs`), commit `chore: sync match data [automated]`.
- **Build status monitor** — every 15 min, scan the codebase vs `specs/`, mark each slice/rule Done/In-Progress/Missing, overwrite `STATUS.md` (<50 lines).

---

## 5. The `/loop` command
Runs a prompt continuously until complete.
- **QA loop (2:15):** `/loop Read the external holdout vault (~/Developer/Hackathon-holdout). Find the next scenario not marked PASS/FAIL, test it against the running app, mark it. Stop when all are marked.`
- **Polish loop (2:40):** `/loop Find one remaining slop issue (placeholder text, missing label, broken 375px layout, unhelpful error) and fix it. Stop when none remain.`

---

## Phase checklist
| Time | Who | Action |
|---|---|---|
| 0:00 | Spec Commander | Confirm SPEC.md + specs/ resolved; start build-status routine |
| 0:00 | QA Auditor | Keep the holdout vault **outside** the repo; never show it to build agents |
| 0:45 | Backend | Fire parallel slices via create-feature (auth, scoring, matches) → write-unit-tests |
| 0:45 | Frontend | Fire parallel screens (auth; calendar+slip; leaderboard) |
| 1:30 | Backend | `/check-br 001-015` |
| 2:00 | Frontend | `/slop-check` |
| 2:00 | QA Auditor | `/check-br 016-028` |
| 2:15 | QA Auditor | Start QA loop against the external vault |
| 2:40 | Demo Lead | `/seed-demo` |
| 2:45 | Demo Lead | `/demo-script` |
| 2:50 | Demo Lead | `/slop-check` final |
| 2:55 | All | Demo rehearsal — no code inspection |
