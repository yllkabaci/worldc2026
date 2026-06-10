# CLAUDE.md — World Cup 2026 Prediction App

Read by Claude Code at the start of every session. It defines how agents behave in this repository. **Read `SPEC.md` before starting any task.**

> Adapted from a team draft and reconciled to this repo's decided architecture. Where this file and a `.claude/rules/*` differ on a coding convention, **the rule wins**.

---

## Project
A World Cup 2026 prediction & scoring platform. The full PRD is `SPEC.md` (index) → `specs/` (platform + per-feature). Domain truth: `WorldCup2026 BusinessLogic EN.docx`.

## Stack (decided)

| Layer | Technology |
|---|---|
| Backend | **.NET 10** ASP.NET Core **Minimal APIs** (no MVC controllers) |
| Pattern | **MediatR CQRS** (`ICommand`/`IQuery`), vertical slices |
| Persistence | **EF Core** — SQLite for the run, SQL Server prod target |
| Auth | **JWT Bearer** + policies (`User`/`Admin`, from the user's `IsAdmin` flag) |
| Errors | **Typed domain exceptions → RFC 7807** (`WC-NNNN`) |
| Frontend | **React 18 + TypeScript + Vite** |
| Server state | **TanStack Query** (no `useEffect` fetching) |
| Forms | React Hook Form + Zod (mirrors FluentValidation) |
| Testing | xUnit + FluentAssertions + Moq (.NET); Vitest (React) |

## Repository structure
```
SPEC.md                     PRD index
specs/                      00-platform.md + per-feature specs (the build input)
backend-architecture.md     frontend-architecture.md
application/backend/        .NET solution (src/ + tests/) + .claude/rules,skills,agents
application/frontend/       React + Vite SPA + .claude/rules,skills
.claude/commands/           team-wide slash commands
```
Holdout verification scenarios live **outside this repo** (`~/Developer/Hackathon-holdout`) and are never shown to build agents.

## Rules every agent must follow

### General
- Read `SPEC.md` fully before any task. Never implement what the spec doesn't describe. Never add libraries without stating which and why. No `TODO` comments — implement it or halt and ask.
- **Halt on ambiguity — never guess** (spec-to-architecture gate). If the code must be read to understand the product, the spec failed.

### Backend (binding rules in `application/backend/.claude/rules/`)
- **No MVC controllers.** Endpoints are Minimal-API static classes (`Map` + `HandleAsync`), one per file, auto-discovered via `IFeatureModule`/`IEndpointModule`.
- Endpoints are transport only: bind → `ISender.Send(command/query)` → wrap in `ApiResponse<T>`. **No logic or data access in endpoints.**
- All business logic lives in the **domain** (aggregates, value objects, the scoring service). Handlers orchestrate and depend on **abstractions** (`IApplicationDbContext`, `IClock`, `ICurrentUserService`) — never `HttpContext`.
- **Scoring** is a pure, deterministic function; points are **`decimal`** (never `double`/`float`), never negative.
- Failures throw **typed domain exceptions** (carry `ErrorCodes`); the global handler maps to RFC 7807. **Do not** use a `Result`/`Outcome` type.
- Handlers never call `SaveChanges` — the UnitOfWork behavior commits commands. Queries are read-only with `AsNoTracking()`.
- `cancellationToken` (full name) is propagated end to end. No raw SQL — EF Core only.
- All endpoints require authorization unless the spec marks them public; admin endpoints use the `Admin` policy.

### Frontend (binding rules in `application/frontend/.claude/rules/`)
- Components are presentation only; data/logic live in feature hooks (TanStack Query). No `useEffect` fetching, no Axios in components.
- Strict TypeScript, no `any`. Types map to backend DTOs; unwrap the `ApiResponse<T>` envelope. No hardcoded match/team/player data — all from the API.
- WCAG 2.1 AA: ARIA labels, ≥4.5:1 contrast, visible focus, 44×44px tap targets, works at 375px. Skeletons on every load — no blank states.

### Forbidden
No controllers, no `Result<T>` pattern, no `double`/`float` for points, no `console.log` in production paths, no `any`, no inline styles, no raw SQL, no hardcoded connection strings/secrets.

## Business rules — frequently missed
- **Scoring** awards points for only two outcomes: exact score = `3`, correct winner/draw = `1`, miss = `0`. **No bonuses, no stage multipliers** (removed from the product).
- **BR-002** points never negative. **BR-003** tiebreak = exact-score accuracy, then earliest registration.
- **BR-007** no edit after the deadline, even if the match is delayed.
- **BR-008** cancelled match → predictions Void (0 points, no penalty).
- **BR-022** all admin changes → immutable audit log.
- Knockouts score on the **regulation 90-minute** result.

## MVP scope (spine)
Auth (minimal JWT) → Matches (calendar + admin result + cancel/postpone + audited re-settlement) → Predictions (create/modify, deadline) → Scoring (exact score / winner / draw) + Settlement → Leaderboard (tiebreak + filters). Groups, real-time/SignalR, notifications, analytics = tier 2.

## Definition of Done
Feature matches its `specs/features/*` spec · business rules enforced in the domain · unit tests pass (scoring exhaustively) · passes its external holdout scenarios · no TS errors · no hardcoded data · ARIA present · mobile works at 375px.
