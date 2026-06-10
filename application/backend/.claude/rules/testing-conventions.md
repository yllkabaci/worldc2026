---
paths:
  - "tests/**/*.cs"
---

# Testing Conventions

## AAA Pattern
ALWAYS structure tests as Arrange / Act / Assert with section comments. Assertions use FluentAssertions; test doubles use Moq.

```csharp
[Fact]
public async Task Handle_PredictionBeforeDeadline_StoresPrediction()
{
    // Arrange
    var command = PredictionFactory.ValidMakePredictionCommand();
    var handler = new MakePredictionHandler(db, currentUser, clock);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
}
```

## What to test where

| Layer | Unit tests | Integration tests |
|-------|-----------|-------------------|
| **Scoring engine** (`Domain/Scoring`) | **Exhaustive** - the crown jewel. Every outcome (exact/winner/draw/miss), cancelled-match void, clamp-at-zero, decimal precision | n/a |
| **Domain aggregates** | Primary - every invariant and lifecycle transition (deadline, state machine, one-prediction-per-match) | n/a |
| **MediatR handlers** | Primary - orchestration paths, success + each thrown domain exception | covered via endpoint integration |
| **Validators** | Unit-test the input rules | wiring test (send invalid request -> 400) |
| **Endpoints** | Only if branching logic; skip for simple pass-through | **Primary** coverage |

### Integration coverage per endpoint
- One test per `Produces<T>()` success response.
- One test per `ProducesProblem()` status (404, 409, 503, ...).
- One test for the validation path (`.ProducesValidationProblem()`): invalid request -> `400` with the `errors` structure.

## Integration test host
- Use **`WebApplicationFactory`** to run the API in-process, backed by **SQLite (or EF in-memory)** - no Docker.
- Override the clock with a controllable `IClock` so deadline/lock behavior is deterministic.
- Replace the external football API with its deterministic twin (no live calls).

## Determinism for scoring
- Scoring tests call the pure function directly with fixed inputs (see `scoring-engine.md`); no mocks, no clock, no DB. Prefer `[Theory]` with `[InlineData]` to cover the matrix of outcomes.

## Test data factories
- Reused fixtures go in a shared `Tests.Helpers/TestData/{Object}Factory.cs` - **static** class, **use-case-named** methods (`ValidMakePredictionCommand`, `SettledMatch`, `ExactScorePrediction`), optional params with sensible defaults.
- Check existing factories before creating fixtures inline.

## Naming & organization
- Test names: `MethodUnderTest_Scenario_ExpectedBehavior`.
- One test class per implementation class; when a file grows large, split by scenario into a folder named after the class, using `partial` classes.

## Test projects
- `tests/WorldCup.Domain.Tests.Unit` - domain + scoring engine (the bulk of coverage).
- `tests/WorldCup.Api.Tests.Unit` - handlers/validators where unit-level.
- `tests/WorldCup.Api.Tests.Integration` - WebApplicationFactory + SQLite.
- `tests/WorldCup.Tests.Helpers` - shared fixtures/factories.
