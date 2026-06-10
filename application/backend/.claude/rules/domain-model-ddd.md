---
paths:
  - "src/**/Domain/**/*.cs"
---

# Domain Model (DDD)

The domain is the heart of the system and owns all business invariants (see `business-rule-placement.md`). It has **no dependencies** on EF Core, ASP.NET, MediatR, or any infrastructure.

## Aggregates
- Model **rich, behavior-bearing aggregates** - never anemic data bags.
- All state uses `private set;` (or `init`); state changes only through **intention-revealing methods** that enforce invariants.
- Construction goes through a **static factory method** (`Match.Schedule(...)`, `Prediction.Place(...)`); constructors are `private`/`protected`. EF Core uses a private constructor.
- A method that breaks an invariant throws a **typed domain exception** (see `error-codes.md`), e.g. `PredictionWindowClosedException`.

```csharp
public sealed class Prediction : AggregateRoot<PredictionId>
{
    public MatchId MatchId { get; private set; }
    public UserId UserId { get; private set; }
    public Score Score { get; private set; }

    private Prediction() { } // EF

    public static Prediction Place(MatchId matchId, UserId userId, Score score, DateTimeOffset now, DateTimeOffset deadline)
    {
        if (now >= deadline) throw new PredictionWindowClosedException(matchId);
        var p = new Prediction { Id = PredictionId.New(), MatchId = matchId, UserId = userId, Score = score };
        p.Raise(new PredictionPlacedEvent(p.Id));
        return p;
    }

    public void Revise(Score score, DateTimeOffset now, DateTimeOffset deadline)
    {
        if (now >= deadline) throw new PredictionWindowClosedException(MatchId);
        Score = score;
        Raise(new PredictionRevisedEvent(Id));
    }
}
```

## Aggregate boundaries
- **`Match`** (root): teams, stage, kickoff, `PredictionDeadline`, status, official `Result`; methods `Schedule`, `OpenForPredictions`, `Settle`, `Cancel`, `Postpone`.
- **`Prediction`** (separate root): references `MatchId` + `UserId`, holds `Score`. Modeled as its **own aggregate root** so a single prediction write never loads or locks the match's full prediction set.
- **`ScoringRuleSet`** (root): effective-dated, immutable once published (see `scoring-engine.md`).
- Reference other aggregates **by id**, never by navigation property across aggregate roots.

## Strongly-typed IDs
- Each aggregate has a value-object id: `public readonly record struct MatchId(Guid Value) { public static MatchId New() => new(Guid.NewGuid()); }`.
- Methods and DTOs use the typed id (`MatchId`, `UserId`, `PredictionId`) - never bare `Guid` - to prevent mixing ids at compile time.
- EF Core maps them with **value converters** (configured in Infrastructure, not Domain).

## Value Objects
- Immutable, equality-by-value (`readonly record struct` or `sealed record`): `Score(int Home, int Away)`, `InviteCode`, `PointsBreakdown`.
- Value objects validate themselves on creation (e.g. `Score` rejects negative or >20 goals) and contain no I/O.

## Domain Events
- Aggregates raise events via a protected `Raise(...)`; events are `sealed record`s implementing `IDomainEvent`.
- Events are **dispatched after** `SaveChangesAsync` by the UnitOfWork behavior (see `mediatr-cqrs.md`), as MediatR `INotification`s - so side effects (leaderboard refresh, notifications) stay decoupled from the aggregate.
- Aggregates never call infrastructure directly.

## Boundary
- Domain entities **never** cross the API boundary. Handlers map aggregates to response DTOs (see `dtos-records.md`).
