---
paths:
  - "src/**/Features/**/*Command*.cs"
  - "src/**/Features/**/*Query*.cs"
  - "src/**/Features/**/*Handler*.cs"
  - "src/**/Behaviors/**/*.cs"
---

# CQRS with MediatR

Every use case is a MediatR request. **Commands** change state; **queries** read state. They are explicitly distinguished by marker interfaces, not just naming.

## Marker Interfaces
```csharp
public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface IQuery<out TResponse> : IRequest<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>;
public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>;
```

- Writes implement `ICommand<T>` / `ICommandHandler<,>`; reads implement `IQuery<T>` / `IQueryHandler<,>`.
- Name files `{UseCase}Command`/`{UseCase}Query` and `{UseCase}Handler` (see `vertical-slice-architecture.md`).
- Commands and queries are `sealed record`s (see `dtos-records.md`). They carry no `HttpContext` (see `handler-no-httpcontext.md`).

## Handlers
```csharp
public sealed class MakePredictionHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IClock clock)
    : ICommandHandler<MakePredictionCommand, MakePredictionResponse>
{
    public async Task<MakePredictionResponse> Handle(MakePredictionCommand cmd, CancellationToken cancellationToken)
    {
        var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == cmd.MatchId, cancellationToken)
            ?? throw new NotFoundException(ErrorCodes.MatchNotFound, "Match not found");

        match.PlacePrediction(currentUser.UserId, cmd.Score, clock.UtcNow); // domain enforces invariants

        // NO SaveChanges here - the UnitOfWork behavior commits (see below)
        return new MakePredictionResponse(/* ... */);
    }
}
```

- Handlers depend only on **abstractions** (`IApplicationDbContext`, `IClock`, `ICurrentUserService`, external-API interfaces) - never concrete infrastructure.
- Business rules live in the **domain**, not the handler (see `business-rule-placement.md`). The handler orchestrates: load -> invoke domain -> map response.
- Failures are signalled with typed domain exceptions mapped to ProblemDetails (see `error-codes.md`).

## Queries are read-only
- Query handlers use **`AsNoTracking()`** and project **straight to response DTOs** (`Select` into the record) - no entity hydration, no change tracking (see `async-patterns.md`).
- Query handlers **never** mutate state and are **not** subject to the UnitOfWork commit.

## Pipeline Behaviors (fixed order)
Cross-cutting concerns are `IPipelineBehavior<,>`, registered outermost-first:

1. **LoggingBehavior** - logs request name + outcome with structured properties.
2. **ValidationBehavior** - runs FluentValidation; short-circuits to a `400` ProblemDetails on failure (see `fluent-validation.md`). Applies to commands and queries.
3. **UnitOfWorkBehavior** - for `ICommand<>` only: after the handler succeeds, calls `IApplicationDbContext.SaveChangesAsync(cancellationToken)` exactly once and dispatches domain events. Queries bypass it.

Handlers therefore **never** call `SaveChanges` themselves; a command = one commit.

## Conventions
- One request + one handler per use case; no "fat" handlers spanning multiple use cases.
- `CancellationToken` is propagated into every async call (`Handle(..., CancellationToken cancellationToken)` -> EF Core / external calls).
- Endpoints map the request DTO to the command/query and `Send` it via `ISender` (see `minimal-api-endpoints.md`).
