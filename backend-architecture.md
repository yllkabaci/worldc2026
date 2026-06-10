# Backend Architecture Specification

**Project:** World Cup 2026 Prediction App — Backend API
**Audience:** the build agent and the engineering team
**Status:** Architecture baseline (v1.1, Hackathon Edition — reconciled with `.claude/rules`)
**Source of truth for domain rules:** `WorldCup2026 BusinessLogic EN.docx`
**Enforced coding conventions:** `application/backend/.claude/rules/*` (see §14)

> This document defines **how** the backend is built (structure, patterns, cross-cutting concerns). The business-logic doc defines **what** it does. The `.claude/rules` files are the binding, file-level conventions; where this document and a rule appear to differ, **the rule wins** and this document should be corrected.

---

## 1. Architectural Drivers

The backend must satisfy a configurable, audited, fairness-critical scoring domain:

- **Correctness is the mission.** Points are money. Scoring must be deterministic, testable in isolation, and reproducible.
- **Rules are configurable and versioned.** Admins change point values and the prediction window. Per the business doc (§8.2), *past matches are scored by the rules that were active at the time* — so scoring rules are **effective-dated** and a match is pinned to the rule set effective **as of its kickoff** (see §11).
- **Two account types.** Regular users vs. administrators, distinguished by a boolean `IsAdmin` flag and enforced by authorization policies.
- **External data is untrusted and may be unavailable.** Fixtures/results arrive from an external football API; the system must degrade gracefully and allow admin override.
- **Auditability.** All administrative changes are written to an immutable audit log.

These drivers justify the layered separation, CQRS, the versioned scoring aggregate, and the observability stack below.

---

## 2. Key Architectural Decisions

| # | Decision | Choice | Rationale |
|---|----------|--------|-----------|
| AD-1 | Macro structure | **Clean Architecture, 3 projects** (Domain / Infrastructure / Api) | Clear dependency rule; slices live in Api (AD-2). |
| AD-2 | Feature organisation | **Vertical Slice**, slices **co-located in `WorldCup.Api/Features`** | Each use case self-contained; one place to read a feature. |
| AD-3 | Endpoint style | **Minimal APIs, REPR, one endpoint per file** — **no MVC controllers** | Thin transport layer. |
| AD-4 | Routing/module glue | **`IFeatureModule` + `IEndpointModule` auto-discovery** (assembly scan) | No manual `Program.cs` wiring; no third-party routing dependency. (Carter/FastEndpoints rejected — see note.) |
| AD-5 | Application mediation | **MediatR** with **`ICommand<T>` / `IQuery<T>`** markers | Endpoints map request → command/query → `ISender.Send`. |
| AD-6 | Validation | **FluentValidation** in a **MediatR `ValidationBehavior`** | Centralised; returns RFC 7807 `400`. Validators check input shape only. |
| AD-7 | Persistence | **EF Core** (SQLite for the hackathon run; SQL Server is the prod target) | Parameterised queries, migrations, `AsNoTracking` reads. |
| AD-8 | AuthN/AuthZ | **JWT Bearer** + named **authorization policies** (`User` / `Admin`, from the user's `IsAdmin` flag) | `RequireAuthorization("Admin")` etc. |
| AD-9 | Errors | **Typed domain exceptions** → **`IExceptionHandler`** → RFC 7807 ProblemDetails | One global path; error codes `WC-NNNN`. |
| AD-10 | Transactions | **`UnitOfWorkBehavior`** commits after a successful command | Handlers never call `SaveChanges`; one commit per command. |
| AD-11 | Observability | **Serilog** (structured JSON) + **OpenTelemetry** + **Health Checks** + correlation id | Operable from day one. See `.claude/rules/observability.md`. |
| AD-12 | API docs | **Swagger UI via Swashbuckle** (OpenAPI v3) with a JWT bearer security scheme | Interactive docs + an `Authorize` button in Development; `/swagger/v1/swagger.json` feeds the frontend's type generation. Served only in Development. |

> **Why not Carter or FastEndpoints (AD-4).** Both implement REPR, but MediatR (AD-5) already owns the handler abstraction, and we want zero third-party routing dependencies for a time-boxed build. Endpoints are plain Minimal-API static classes (`Map` + `HandleAsync`); each feature's `IEndpointModule` maps its endpoints, and modules are auto-discovered by assembly scan. See `.claude/rules/minimal-api-endpoints.md` and `vertical-slice-architecture.md`.

---

## 3. Solution Structure (Clean Architecture, 3 projects)

```
/application/backend
  WorldCup.sln
  src/
    WorldCup.Domain/          # Enterprise rules — NO external/framework dependencies
      Common/                 #   AggregateRoot, IDomainEvent, ValueObject, strongly-typed ID base
      Abstractions/           #   IApplicationDbContext, IClock, ICurrentUserService, IFootballApi
      Matches/                #   Match (root), MatchStatus, Score (VO), domain events
      Predictions/            #   Prediction (root)
      Scoring/                #   ScoringRuleSet (versioned), PointsBreakdown (VO), ScoringService (pure)
      Users/                  #   User (IsAdmin flag), AccountStatus
      Groups/                 #   Group, InviteCode (VO), Membership
      Audit/                  #   AuditLogEntry
      Exceptions/             #   DomainException base + typed exceptions, ErrorCodes
    WorldCup.Infrastructure/  # Implements Domain abstractions — depends on Domain only
      Persistence/            #   ApplicationDbContext : IApplicationDbContext, EF configs, migrations, value converters
      Identity/               #   JWT issuer, password hashing, OAuth handlers, CurrentUserService
      ExternalApis/           #   FootballApiClient + deterministic twin/stub
      Time/                   #   SystemClock : IClock
    WorldCup.Api/             # Composition root + ALL feature slices
      Features/               #   VERTICAL SLICES, co-located (see §4)
        Predictions/
          PredictionsModule.cs          # IFeatureModule + IEndpointModule
          MakePrediction/               # Endpoint + Command + Handler + Validator + Response
          ModifyPrediction/
          GetMyPredictions/
        Matches/ ... Leaderboard/ ... Groups/ ... Admin/ ...
      Common/                 #   Behaviors (Logging/Validation/UnitOfWork), ApiResponse<T>,
                              #   GlobalExceptionHandler, RouteNames, module scanning, JSON defaults
      Program.cs
  tests/
    WorldCup.Domain.Tests.Unit     # Domain + scoring engine (the bulk of coverage)
    WorldCup.Api.Tests.Unit        # Handlers/validators where unit-level
    WorldCup.Api.Tests.Integration # WebApplicationFactory + SQLite
    WorldCup.Tests.Helpers         # Shared fixtures/factories
```

**Dependency rule (strict):** `Api → Infrastructure → Domain`. Domain references nothing. Abstractions that Infrastructure implements are declared in `Domain/Abstractions`. Handlers (in `Api`) depend on those **abstractions**, never on concrete infrastructure. The DI container in `Api` is the single composition root; **no Service Locator** anywhere.

---

## 4. Vertical Slices & the REPR Bundle

Each use case from the business doc maps to one slice, **co-located** in `WorldCup.Api/Features/{Feature}/{UseCase}/`. Use cases sit directly under the feature — no `Commands/`/`Handlers/` grouping. (See `.claude/rules/vertical-slice-architecture.md`.)

```
Features/Predictions/
  PredictionsModule.cs               # IFeatureModule (DI) + IEndpointModule (maps the endpoints below)
  MakePrediction/
    MakePredictionEndpoint.cs        # static Map + HandleAsync
    MakePredictionCommand.cs         # ICommand<MakePredictionResponse> (sealed record)
    MakePredictionHandler.cs         # ICommandHandler<...> — loads aggregate, calls domain
    MakePredictionValidator.cs       # AbstractValidator<MakePredictionCommand> (input shape only)
    MakePredictionResponse.cs        # sealed record
```

Endpoint (REPR, one per file) — see `.claude/rules/minimal-api-endpoints.md`:

```csharp
public static class MakePredictionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/matches/{matchId:guid}/prediction", HandleAsync)
            .WithName(RouteNames.MakePrediction)
            .RequireAuthorization("User")
            .Produces<ApiResponse<MakePredictionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid matchId,
        [FromBody] MakePredictionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(request.ToCommand(matchId), cancellationToken);
        return Results.Ok(response.ToApiResponse());
    }
}
```

The endpoint is **transport only**: bind → map to command → `Send` → wrap in `ApiResponse<T>`. No business logic, no data access in the endpoint.

### Slice inventory (mapped to business-doc use cases)

| Slice group | Commands / Queries | Auth policy |
|-------------|--------------------|------|
| Auth | Register, Login, OAuthCallback, ResetPassword (UC-U01–03) | anonymous |
| Profile | UpdateProfile, GetMyHistory (UC-U04–05) | User |
| Predictions | MakePrediction, ModifyPrediction, GetMyActivePredictions (UC-U06–08) | User |
| Matches | GetMatchCalendar, GetMatchDetails, GetGroupStandings (UC-U11–13) | anonymous/User |
| Leaderboard | GetGlobalLeaderboard, GetMyRanking (UC-U09–10) | User |
| Groups | CreateGroup, JoinGroup, GetGroupLeaderboard (UC-U14–16) | User |
| Scoring | SettleMatch (internal command triggered by SetOfficialResult) | system |
| Admin · Matches | SetOfficialResult, CancelOrPostponeMatch (UC-A10–11) | Admin |
| Admin · Config | ConfigurePointsSystem, ConfigurePredictionWindow, ConfigureGroupRules (UC-A03–05) | Admin |
| Admin · Users | ListUsers, BlockOrActivate, PromoteToAdmin (UC-A07–09) | Admin |
| Admin · Reports | AnalyticsDashboard, ExportData (UC-A12–13) | Admin |

---

## 5. Patterns & Conventions

### 5.1 CQRS with MediatR
- Writes implement **`ICommand<T>`** / **`ICommandHandler<,>`**; reads implement **`IQuery<T>`** / **`IQueryHandler<,>`** (see `.claude/rules/mediatr-cqrs.md`).
- Handlers depend on **abstractions** (`IApplicationDbContext`, `IClock`, `ICurrentUserService`, `IFootballApi`), never concrete infrastructure, and never on `HttpContext` (see `handler-no-httpcontext.md`).
- Cross-cutting concerns run as **pipeline behaviors** in order: `LoggingBehavior` → `ValidationBehavior` → `UnitOfWorkBehavior` → handler.
- **Handlers never call `SaveChanges`** — the `UnitOfWorkBehavior` commits once after a successful command and dispatches domain events. Queries are read-only, use `AsNoTracking()`, and project straight to response DTOs.

### 5.2 Domain-Driven Design
- **Rich aggregates** with `private set;` and behavior methods; construction via static factory methods; **strongly-typed IDs** (`MatchId`, `UserId`, `PredictionId`). See `.claude/rules/domain-model-ddd.md`.
  - `Match` (root): status, kickoff, `PredictionDeadline`, official `Result`, **pinned `ScoringRuleSetId`**; methods `Schedule`, `OpenForPredictions`, `Settle`, `Cancel`, `Postpone`. Lifecycle `Upcoming → Live → Finished/Cancelled`.
  - `Prediction` (**its own aggregate root**): references `MatchId` + `UserId`; `Score`; `Place`/`Revise` allowed only before the deadline.
  - `ScoringRuleSet` (root, **versioned/effective-dated**): immutable once published; a match is scored by the rule set effective **as of its kickoff** (business doc §8.2).
- **Value objects:** `Score(Home, Away)`, `InviteCode`, `PointsBreakdown` — immutable, equality by value.
- **Domain events** are raised by aggregates and dispatched **after** `SaveChanges` (by the UnitOfWork behavior) to trigger side effects without coupling the aggregate to infrastructure.
- **DTOs are separate** from entities — entities never cross the API boundary (see `dtos-records.md`).

### 5.3 Dependency Injection
- Explicit lifetimes: `AddScoped` for `DbContext` and handlers, `AddSingleton` for `IClock`/stateless config, `AddTransient` for validators. **No Service Locator.**
- Each layer ships an `Add{Layer}` extension (`AddInfrastructure`, plus feature `IFeatureModule.ConfigureServices`) wired in `Program.cs`; feature modules are auto-discovered.

---

## 6. Security

- **Authentication:** JWT Bearer. Identities originate from local registration (hashed passwords, BR-017) or OAuth providers (Google/Facebook, UC-U02). An `IJwtIssuer` mints tokens carrying `sub`, `role`, and `email` claims.
- **Authorization policies:** `"User"` (any authenticated) and `"Admin"` (the user's `IsAdmin` flag), registered once and applied per endpoint via `.RequireAuthorization("…")`. The JWT `role` claim is `Admin` or `User`, derived from `IsAdmin` at login. Blocked accounts (BR-009/BR-018) fail the check.
- **Current user in handlers** via `ICurrentUserService` (backed by `IHttpContextAccessor`), never by reading `HttpContext` in a handler (see `handler-no-httpcontext.md`).
- **Input validation:** FluentValidation in the `ValidationBehavior` (input shape only; ranges per BR-010). Business invariants live in the domain (see `business-rule-placement.md`).
- **Data protection:** EF Core parameterised queries only; passwords hashed (never logged); PII masked in logs. Lockout after 5 failed logins for 15 min (BR-018); sessions expire after 24h (BR-019).
- **Audit:** every admin command appends an immutable `AuditLogEntry` (BR-022).

See the `security-reviewer` agent for the review checklist.

---

## 7. Error Handling

- Failures are signalled with **typed domain exceptions** carrying an `ErrorCodes` value (`WC-NNNN`, ranged by concern). A global `GlobalExceptionHandler : IExceptionHandler` maps them to **RFC 7807 ProblemDetails** via a single code→status table. Handlers/domain throw; they never build ProblemDetails by hand. See `.claude/rules/error-codes.md`.
- Status mapping: validation → `400`, unauthenticated → `401`, forbidden → `403`, not found → `404`, business-rule conflict (e.g. predicting after the deadline, BR-007) → `409`, external API down → `503`, unexpected → `500`.
- Validation failures come from the `ValidationBehavior` as a `400` ProblemDetails with the `errors` dictionary. No raw stack traces leak to clients.

---

## 8. Observability

Full conventions: `.claude/rules/observability.md`.

- **Logging:** Serilog with structured properties and JSON-friendly sink. **Never** interpolate — `logger.LogInformation("User {UserId} predicted match {MatchId}", userId, matchId)`. No secrets/PII in logs.
- **Correlation id:** `CorrelationIdMiddleware` reads/generates `X-Correlation-Id`, echoes it on the response, and pushes it onto the Serilog `LogContext`; it also propagates on outbound `HttpClient` calls.
- **Tracing/metrics:** OpenTelemetry (ASP.NET Core + HttpClient + runtime instrumentation) via OTLP, plus the app `ActivitySource` (`Telemetry.Source`) for custom spans around the scoring engine and external provider calls.
- **Health:** `/healthz` (liveness) and `/readyz` (readiness — a `DbContext` check tagged `ready`; add an external-provider check when live football-data.org is enabled).

---

## 9. Performance & Reliability

- **Read path:** queries `AsNoTracking()`, project straight to response DTOs.
- **`ValueTask`** for hot, often-synchronous abstractions (clock/cache).
- **`cancellationToken`** (full name) propagates from every endpoint delegate → MediatR handler → EF Core / `HttpClient`. No sync-over-async. See `.claude/rules/async-patterns.md`.
- **External football API** behind `IFootballApi` — provider **football-data.org** (v4, `X-Auth-Token` header, competition code `WC`), implemented by `FootballDataOrgClient` (typed `HttpClient`, timeout + retry). A **deterministic twin** is used for local/dev/tests and when no API token is configured (no live calls during the build). Admin override wins over imported data (UC-A02/§8.3). Scoring needs the regulation 90-minute score — verify the provider's ET field mapping (see `specs/features/02-matches.md`).

---

## 10. Request Lifecycle (happy path)

```
HTTP request
  → Minimal-API endpoint (bind DTO, RequireAuthorization, map to command/query)
  → ISender.Send(command/query, cancellationToken)
    → LoggingBehavior
    → ValidationBehavior (FluentValidation; 400 ProblemDetails on failure)
    → UnitOfWorkBehavior (commands only)
      → Handler (loads aggregate via IApplicationDbContext)
        → Domain method enforces invariants (may raise domain events)
      → SaveChangesAsync(cancellationToken) once after the handler
      → Domain events dispatched (leaderboard refresh / notifications)
  → Response DTO → response.ToApiResponse() → Results.Ok / Created
Unhandled / domain exception anywhere → GlobalExceptionHandler → ProblemDetails (RFC 7807)
```

---

## 11. Scoring Engine (the correctness core)

Scoring is isolated and pure — see `.claude/rules/scoring-engine.md`:

- A `ScoringService` in `Domain/Scoring` takes a `Prediction`, the official `MatchResult`, and the **pinned `ScoringRuleSet`**, and returns a `PointsBreakdown` — a **pure function** with no I/O, clock, or randomness.
- Algorithm: points for one of two outcomes (configurable) — **exact score `3`**, **correct winner/draw `1`**, otherwise `0`. There are **no bonuses and no stage multipliers**.
- **Points are `decimal`** (never `double`/`float`), with **no rounding**. Points are **never negative** (clamp at 0, BR-002).
- A match is scored by the rule set effective **as of its kickoff** (pinned onto the `Match`).
- Void cases: cancelled match → predictions **void** — 0 points, no penalty (BR-008).
- Determinism enables exhaustive unit tests and behavioral holdout scenarios that verify scoring without reading code. See the `business-rules-reviewer` agent.

---

## 12. Hackathon Scope & Build Order (pragmatic)

The full domain is large (104 matches, groups, admin panel, notifications, analytics, mobile). Build the spine first:

1. **Core spine (must):** Auth (JWT), Matches (calendar + admin set-result), MakePrediction/ModifyPrediction with the deadline rule, the **Scoring engine + SettleMatch**, Global Leaderboard with tiebreak (BR-003: exact-score accuracy).
2. **Second tier (if time):** versioned points configuration (admin), group standings.
3. **Defer:** private groups, notifications, analytics/export, OAuth providers (use local JWT), mobile, real football-API sync (use the twin).

Verifiable core: *predict → settle → score → rank*.

---

## 13. Resolved Decisions / Assumptions

All decisions are settled — `SPEC.md §11` is the single source. Highlights: routing via auto-discovered modules (AD-4); 3 projects with co-located slices; `ICommand`/`IQuery` markers; mandatory UnitOfWork behavior; decimal points; **scoring awards points only for an exact score (`3`) and a correct winner/draw (`1`)** (no bonuses, no stage multipliers); **single mutable `ScoringRuleSet`** (effective-dated history tier 2); knockouts scored on the **regulation 90-minute** result; **minimal auth** (active on registration, no verification/lockout, symmetric dev key); **distinct create/modify** prediction verbs; **cancel/postpone, re-settlement-with-audit, and leaderboard filters pulled into the MVP**; integration tests on SQLite; runtime **.NET 10 (LTS)**.

> If a *new* ambiguity surfaces during the build, halt and clarify — do not guess.

---

## 14. Related Documents (cross-references)

- **Enforced coding conventions:** `application/backend/.claude/rules/` — `vertical-slice-architecture`, `minimal-api-endpoints`, `mediatr-cqrs`, `mediatr-pipeline`, `handler-no-httpcontext`, `business-rule-placement`, `domain-model-ddd`, `dtos-records`, `fluent-validation`, `error-codes`, `auth-and-authorization`, `json-serialization`, `async-patterns`, `ef-core-persistence`, `observability`, `scoring-engine`, `testing-conventions`. These are binding at the file level; this document is the higher-level overview.
- **Authoring skills:** `application/backend/.claude/skills/create-feature` (scaffold a slice from a description or spec) and `write-unit-tests` (tests per the rules).
- **Review agents:** `application/backend/.claude/agents/` — `code-reviewer`, `architecture-reviewer`, `test-reviewer`, `business-rules-reviewer`, `security-reviewer`.
- **Frontend:** `frontend-architecture.md` — the React SPA that consumes this API. Note: success responses are wrapped in the **`ApiResponse<T>`** envelope, which the SPA's HTTP layer unwraps; failures are RFC 7807 ProblemDetails.
- **Domain source of truth:** `WorldCup2026 BusinessLogic EN.docx`.
- **Scope note:** observability (§8) and external HTTP-client/config concerns are described here as the vision but were intentionally left out of the *core* rule set; add rules for them if/when those areas are built.
