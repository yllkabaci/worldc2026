# WorldCup 2026 Prediction API — Backend

Spec-generated **Step 0 skeleton** (zero features). Generated from `specs/00-platform.md`,
`backend-architecture.md`, and `.claude/rules/`. It compiles, serves health checks, and is
ready for the per-slice build loop — it contains **no** features yet.

## Prerequisites
- .NET 10 SDK

## Commands
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/WorldCup.Api   # then GET http://localhost:5080/healthz
```

## Layout
```
src/WorldCup.Domain          # aggregates, value objects, events, abstractions, exceptions (no deps)
src/WorldCup.Infrastructure  # EF Core (SQLite), JWT issuer, password hasher, clock, football twin
src/WorldCup.Api             # Minimal API host + Common (CQRS, behaviors, ApiResponse,
                             #   exception handler, module scanning) + Features/ (added per slice)
tests/                       # Domain/Api unit tests, integration (WebApplicationFactory + SQLite), Tests.Helpers
```

## What's wired (cross-cutting)
- MediatR pipeline: Logging -> Validation (FluentValidation) -> UnitOfWork -> handler
- `ICommand`/`IQuery` markers; handlers depend on abstractions (`IApplicationDbContext`, `IClock`, `ICurrentUserService`)
- JWT bearer + `User`/`Admin`/`SuperAdmin` policies
- Typed exceptions -> RFC 7807 ProblemDetails (`WC-NNNN` error codes)
- `ApiResponse<T>` success envelope; camelCase JSON, enums as strings
- Auto-discovered feature modules (`IFeatureModule` / `IEndpointModule`) — add a feature, no `Program.cs` change
- Health checks at `/healthz`, `/readyz`; OpenAPI in Development; CORS for the Vite dev origin

## Next steps (per-slice build loop)
Run the `create-feature` skill with a spec from `../../specs/features/`, then `write-unit-tests`,
then the review agents. Build order: 01-auth → 02-matches → 03-predictions → 04-scoring-settlement → 05-leaderboard.
