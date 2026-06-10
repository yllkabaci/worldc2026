# World Cup 2026 Prediction App — Product Specification (PRD)

**Status:** v1.0, Hackathon Edition — Dark Factory build
**This file is the entry point.** Read it first. It states what the product is, where every other specification lives, and what "done and correct" means. It aggregates the corpus *by reference* — detail lives in the linked documents, not here.

> **Dark Factory principle.** Specs in → working software out. Everything in §3 is the *input* the build agent consumes; code (including the project skeleton) is an *output* of these specs, never the reverse. If a human has to read the generated code to understand the product, the specification has failed.

---

## 1. Product Vision & Mission

The World Cup 2026 Prediction App lets users predict the results of FIFA World Cup 2026 matches, earn points based on the accuracy of their predictions, and compete on leaderboards. The 2026 tournament is the largest ever — 48 teams, 104 matches across the USA, Canada, and Mexico — which is the engagement opportunity the product is built around.

**Mission for this build:** demonstrate true AI-native execution — a polished, correct, multi-feature product produced from this specification corpus alone, with the implementation treated as a black box and correctness proven by external behavioral scenarios.

---

## 2. Scope

**In scope (MVP spine — must build):** authentication (JWT, minimal), match calendar + details + admin result entry **including cancel/postpone and re-settlement with audit**, create/modify prediction with the deadline rule, the **base** scoring engine + match settlement, and the global leaderboard with tiebreak **and filters (period/stage/country)**. This is the verifiable core: *predict → settle → score → rank*.

**Second tier (if time):** bonus predictions + stage multipliers, configurable points (admin), private groups, my-predictions history, blocked-user removal from the leaderboard.

**Out of scope (this version):** monetary payments/prizes, live match streaming, advanced AI analytics, native mobile, social-network integration beyond image sharing (business doc §9.2).

---

## 3. The Specification Corpus

The PRD is layered. Read in this order; each document owns one concern.

| Layer | Document | Role |
|-------|----------|------|
| Index | `SPEC.md` (this file) | Vision, scope, corpus map, gates, build order |
| Requirements ("what") | `WorldCup2026 BusinessLogic EN.docx` | Authoritative business rules: use cases, points system, prediction/group/security rules |
| Requirements, decomposed | `specs/features/*.md` | Per-feature, agent-grade specs (one per slice) — the unit the build consumes |
| Platform ("build me first") | `specs/00-platform.md` | The skeleton the build generates before any feature |
| Architecture & constraints ("how-bounded") | `backend-architecture.md`, `frontend-architecture.md` | Structure, patterns, tech stack |
| Conventions ("house style") | `application/backend/.claude/rules/*` | Binding, file-scoped coding rules |

**Not part of the PRD (deliberately):**
- **Factory machinery** — `application/backend/.claude/skills/*` (how to build/test) and `.claude/agents/*` (how to review). These operate *on* the spec; they aren't requirements.
- **Verification track** — the holdout behavioral scenarios. They are **external and never shown to the build agent** (see §9). The per-feature specs describe observable behavior, but the concrete holdout scenarios live separately.

---

## 4. Actors & Roles

- **User** — registered participant: predicts, views leaderboards, joins groups.
- **Admin** — manages matches, sets official results, configures rules, manages users.
- **Super Admin** — Admin plus the right to promote other admins.

Enforced by JWT authorization policies `User` / `Admin` / `SuperAdmin` (see `backend-architecture.md §6`).

---

## 5. Architecture & Tech Constraints (locked)

Full detail in `backend-architecture.md` / `frontend-architecture.md`. Locked decisions the build must honor:

- **Backend:** **.NET 10 (LTS)**, ASP.NET Core **Minimal APIs**, **MediatR CQRS** (`ICommand`/`IQuery`), vertical slices co-located in `WorldCup.Api`, **3 projects** (Api / Infrastructure / Domain), auto-discovered feature modules, EF Core (SQLite for the run; SQL Server prod target), JWT + policies (symmetric dev signing key), typed exceptions → RFC 7807, `ApiResponse<T>` success envelope.
- **Frontend:** React + TypeScript + **Vite** SPA, feature folders mirroring backend slices, TanStack Query, React Hook Form + Zod, RFC 7807 parsing, role-aware protected routes.
- **Scoring:** pure deterministic engine, **decimal** points; the MVP uses **base scoring only** (exact/winner/draw) with a **single mutable `ScoringRuleSet`** (matches still pin a rule-set id; effective-dating is tier 2); bonuses and stage multipliers are tier 2; knockouts are scored on the **regulation 90-minute** result.

This section is the anchor for **Quality Gate 1 (spec-to-architecture sync)**.

---

## 6. Domain Glossary

- **Match** — a fixture with stage, two teams (may be undetermined until a knockout slot resolves), kickoff, status, and an official result. Lifecycle: `Upcoming → Live → Finished / Cancelled`.
- **Prediction** — a user's predicted regulation score (and optional bonus predictions) for a match. Its own aggregate root.
- **Prediction deadline** — `kickoff − window` (default 60 min, configurable). Predictions are accepted only before it.
- **ScoringRuleSet** — versioned, effective-dated configuration of point values, bonuses, and stage multipliers. A match is scored by the rule set effective **as of its kickoff**.
- **PointsBreakdown** — the itemized result of scoring one prediction (base + bonuses + multiplier).
- **Leaderboard** — users ranked by total points; tiebreak by exact-score accuracy (BR-003).
- **Group** — a private mini-league (tier 2).

---

## 7. Feature Index (slice map / backlog)

| # | Feature spec | Slices | Tier | Auth |
|---|--------------|--------|------|------|
| 00 | `specs/00-platform.md` | solution skeleton | build-step 0 | — |
| 01 | `specs/features/01-auth.md` | Register, Login | spine | anonymous |
| 02 | `specs/features/02-matches.md` | GetMatchCalendar, GetMatchDetails, SetOfficialResult, CancelOrPostpone, Re-settle (audited) | spine | anon/User + Admin |
| 03 | `specs/features/03-predictions.md` | MakePrediction (POST), ModifyPrediction (PUT), GetMyActivePredictions | spine | User |
| 04 | `specs/features/04-scoring-settlement.md` | SettleMatch + base scoring engine | spine | system |
| 05 | `specs/features/05-leaderboard.md` | GetGlobalLeaderboard (+ filters), GetMyRanking | spine | User |
| — | (tier 2) | Bonus predictions, stage multipliers, Groups, Admin config, Reports, blocked-user removal, effective-dated rules | later | — |

---

## 8. Quality Gates

Every feature must pass three gates before it counts as done (from the Dark Factory model):

1. **Spec-to-architecture sync** — the feature spec is consistent with §5 and the `.claude/rules`. If a spec is ambiguous, the build **halts and clarifies** — it does not guess.
2. **Holdout behavioral scenarios** — the feature passes external scenarios it was never shown during the build. On failure, refine the **spec**, not the code by hand.
3. **Integrity & slop audit** — survives the review agents: security, correctness, business-rule placement, scoring correctness, test coverage. No shallow/generic output.

---

## 9. Build Order & Execution Loop

1. **Step 0 — Platform:** generate the skeleton from `specs/00-platform.md` (agent-generated from spec, not hand-written).
2. **Per slice, in spine order (01→05):**
   a. Run the **`create-feature`** skill with the feature spec as input.
   b. Run the **`write-unit-tests`** skill on the result.
   c. Run the relevant **review agents** (architecture, code, business-rules/scoring, security, test).
   d. Run the feature's **holdout scenarios**; on failure, refine the spec and repeat.

The demo-able loop after the spine: *see fixtures → predict → admin settles → leaderboard updates*.

---

## 10. Verification Strategy

- **Holdout behavioral scenarios** — external, deterministic, never given to the build agent. They are the primary proof of correctness (Gate 2).
- **Scoring engine** — exhaustively unit-tested as a pure function (the correctness core).
- **Review agents** — automated integrity/slop audit (Gate 3).
- Correctness is demonstrated through **behavior**, not by reading code.

---

## 11. Resolved Decisions (single source)

All previously open decisions are now settled (resolved with the team):

1. **Auth model** — local JWT, **minimal**: email/password, accounts **active on registration** (no email-verification gate), **no lockout** in the MVP. OAuth, verification, and lockout (BR-018) are tier 2. Symmetric dev signing key.
2. **Database** — **SQLite** for the hackathon run; SQL Server is the prod target.
3. **Rules-versioning depth** — **single mutable `ScoringRuleSet`** for the MVP; matches pin a rule-set id but effective-dating/history is tier 2.
4. **Scoring scope** — **base only** (exact `3` / winner `1` / draw `1` / miss `0`). The seven bonus predictions **and** stage multipliers are tier 2 (the engine is built to support them).
5. **Leaderboard tiebreak** — primary BR-003 (exact-score accuracy), secondary **earliest registration** → unique deterministic winner.
6. **Prediction API** — **distinct verbs**: `POST` create + `PUT/PATCH` modify; a second create for the same match returns `409`.
7. **Knockout scoring** — uses the **regulation 90-minute** result (extra time and shootouts ignored).
8. **Tier-2 pulled into the MVP spine** — **cancel/postpone match**, **re-settlement with double-confirmation + audit**, and **leaderboard filters** (period/stage/country). Blocked-user removal stays tier 2.
9. **Runtime** — **.NET 10 (LTS)**.
10. **Fixture data source** — **football-data.org** (v4 API, `X-Auth-Token` header, competition code `WC`). The MVP uses the deterministic twin for tests/holdout; a real `FootballDataOrgClient` pulls fixtures/results when an API token is configured (live sync remains tier 2). Scoring uses the regulation 90-minute result — the provider field mapping for knockouts that go to extra time must be verified against a live sample.

> Governing principle still applies: if any *new* ambiguity surfaces during the build, halt and clarify — do not guess.

---

## 12. Governing Principles

- **If you have to read the code, the spec failed.** Specs define observable behavior; implementation is a black box.
- **Halt on ambiguity — never guess.** A missing or contradictory rule stops the build for clarification.
- **The rule wins.** Where this PRD and a `.claude/rule` differ on a coding convention, the rule is authoritative and this corpus is corrected.
- **Verify by behavior.** Correctness is proven by holdout scenarios and tests, not by inspection of the implementation.
