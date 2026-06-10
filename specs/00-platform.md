# 00 — Platform / Skeleton Specification

**Type:** Platform spec (build-step 0). **Status:** spine prerequisite.
**Consumes:** `backend-architecture.md` (§3 structure, §5 patterns) + `application/backend/.claude/rules/*`.
**Produces:** the empty, compiling solution skeleton that every feature slice drops into.

> This is generated **first**, before any feature. It is produced *from* this spec (and the architecture doc), not hand-written. No business behavior lives here — only the scaffolding and cross-cutting plumbing the rules require.

---

## System Overview
A compiling .NET solution with three projects (Api / Infrastructure / Domain) and four test projects, wired with the MediatR pipeline, the global exception handler, JSON defaults, auth, EF Core (SQLite), and feature-module auto-discovery — but **zero features**. Running it exposes only health endpoints and an empty route table; `dotnet build` and `dotnet test` succeed.

## What the skeleton must contain

### Solution & projects (`backend-architecture.md §3`)
- `WorldCup.sln` with `src/WorldCup.Domain`, `src/WorldCup.Infrastructure`, `src/WorldCup.Api`.
- Test projects: `WorldCup.Domain.Tests.Unit`, `WorldCup.Api.Tests.Unit`, `WorldCup.Api.Tests.Integration`, `WorldCup.Tests.Helpers`.
- Dependency direction enforced: `Api → Infrastructure → Domain`; Domain references nothing.

### Domain (`domain-model-ddd.md`)
- `Common/`: `AggregateRoot<TId>`, `IDomainEvent`, base for strongly-typed IDs, value-object base.
- `Abstractions/`: `IApplicationDbContext`, `IClock`, `ICurrentUserService`, `IFootballApi` (interfaces only).
- `Exceptions/`: `DomainException` base + `ErrorCodes` enum (seed values per `error-codes.md`).
- No aggregates yet (features add them).

### Infrastructure
- `ApplicationDbContext : IApplicationDbContext` (EF Core, SQLite provider) with strongly-typed-ID value converters wired generically; **no entity configurations yet**.
- `SystemClock : IClock`; `CurrentUserService : ICurrentUserService` (reads the authenticated principal).
- `Identity/`: `IJwtIssuer` + implementation; password hasher.
- `ExternalApis/`: `IFootballApi` client + a deterministic in-memory twin (selected in non-prod/test).

### Api — cross-cutting (`mediatr-cqrs.md`, `minimal-api-endpoints.md`, `error-codes.md`, `json-serialization.md`)
- `Common/Behaviors/`: `LoggingBehavior`, `ValidationBehavior`, `UnitOfWorkBehavior` registered in order Logging → Validation → UnitOfWork → handler.
- `Common/`: `ApiResponse<T>` + `.ToApiResponse()` / `.ToApiListResponse()` extensions; `GlobalExceptionHandler : IExceptionHandler` with the `ErrorCodes → HTTP status` map; `RouteNames` constants class (empty); module-scanning that discovers `IFeatureModule` / `IEndpointModule` across the assembly.
- CQRS marker interfaces: `ICommand<T>`, `IQuery<T>`, `ICommandHandler<,>`, `IQueryHandler<,>`.
- `Program.cs`: JSON defaults (camelCase, enums as strings), JWT bearer + the `User`/`Admin`/`SuperAdmin` policies, MediatR + FluentValidation assembly scan, `AddInfrastructure`, exception handler, health checks, **Swagger / OpenAPI (Swashbuckle) with a JWT bearer security scheme**, feature-module discovery, CORS for the frontend dev origin.

### Tests
- `WorldCup.Tests.Helpers` with an empty `TestData/` and a `WebApplicationFactory` fixture configured for **SQLite/in-memory** with an overridable `IClock` and the football-API twin (per `testing-conventions.md`).

## Behavioral Contract
- `GET /healthz` returns healthy; `GET /readyz` returns ready when the DB is reachable.
- In Development, **Swagger UI is served at `/swagger`** and the OpenAPI document at `/swagger/v1/swagger.json`; the UI has an **Authorize** button for pasting a JWT. (Disabled outside Development.)
- With no features registered, no business routes exist; the app still starts and serves health checks.
- `dotnet build` and `dotnet test` both succeed on the empty skeleton.

## Explicit Non-Behaviors
- The skeleton must **not** contain any feature, aggregate, endpoint, or business rule.
- It must **not** hardcode any scoring/point values (those arrive with features and the rule set).
- It must **not** use MVC controllers, Carter, MediatR `IRequest` without the `ICommand`/`IQuery` markers, or `double`/`float` anywhere money-related.

## Definition of Done
The solution compiles, health checks pass, the MediatR pipeline + exception handler + JSON defaults + auth policies + module discovery are in place, and adding a feature module later requires **no** `Program.cs` change. Verified by a smoke integration test (app boots, `/healthz` OK) and `dotnet build`/`dotnet test` green.

## Resolved Decisions
1. Runtime: **.NET 10 (LTS)**.
2. Dev DB: **SQLite** (file); tests use the `WebApplicationFactory` (SQLite/in-memory).
3. JWT: **symmetric dev signing key**.
