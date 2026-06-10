---
name: write-unit-tests
description: Use when writing or adding tests for the World Cup 2026 backend - a scoring engine, a domain aggregate, a MediatR handler, a validator, or an endpoint. Produces tests that conform to .claude/rules/testing-conventions.md (AAA, FluentAssertions, Moq, factories, correct test projects) and the rule for the code under test. Triggers - "write tests for", "unit test this", "add tests", "test the handler/scoring/validator", "cover this feature with tests".
---

# Write Unit Tests

Write tests for a target (a feature slice, handler, domain type, scoring engine, validator, or endpoint), following the project conventions exactly.

## 0. Load the rules (mandatory)
Always read `.claude/rules/testing-conventions.md`. Then read the rule for the code under test:
- Scoring -> `scoring-engine.md`
- Domain aggregate/value object -> `domain-model-ddd.md`, `business-rule-placement.md`
- Handler -> `mediatr-cqrs.md`, `handler-no-httpcontext.md`, `error-codes.md`
- Validator -> `fluent-validation.md`
- Endpoint -> `minimal-api-endpoints.md`

## 1. Identify the target layer(s)
Read the code under test and classify it. Use the coverage matrix from `testing-conventions.md`:

| Target | What to cover | Project |
|--------|---------------|---------|
| **Scoring engine** | Exhaustive: every base tier, every bonus, void cases (0-0 minute, cancelled), multiplier math, clamp-at-zero, decimal precision | `WorldCup.Domain.Tests.Unit` |
| **Domain aggregate** | Every invariant + lifecycle transition; factory rejects invalid input; events raised | `WorldCup.Domain.Tests.Unit` |
| **Handler** | Happy path + each thrown domain exception; correct domain method called | `WorldCup.Api.Tests.Unit` |
| **Validator** | Valid passes; one failing test per rule (ranges, required) | `WorldCup.Api.Tests.Unit` |
| **Endpoint** | One per `Produces<T>()`, per `ProducesProblem()`, and the validation `400` | `WorldCup.Api.Tests.Integration` |

## 2. Conventions to apply
- **AAA** with `// Arrange / // Act / // Assert` comments.
- **FluentAssertions** for assertions; **Moq** for test doubles.
- Naming: `MethodUnderTest_Scenario_ExpectedBehavior`.
- Reuse/extend factories in `WorldCup.Tests.Helpers/TestData/{Object}Factory.cs` (static, use-case-named methods, optional params). Check existing factories before adding fixtures inline.
- One test class per implementation class; split large files by scenario into a class-named folder using `partial` classes.

## 3. Layer-specific guidance

### Scoring engine (highest priority)
- Test the **pure function directly** - no mocks, no clock, no DB.
- Use `[Theory]` + `[InlineData]` to sweep the outcome/bonus matrix.
- Explicitly cover: exact vs winner vs draw vs miss; each bonus on/off; minute-of-first-goal void on 0-0; cancelled-match void; penalty excludes shootouts; multiplier applied to the match total; never-negative clamp; `decimal` precision (no rounding).

### Domain aggregates
- One test per invariant and transition (e.g. predicting after the deadline throws `PredictionWindowClosedException`; illegal match-status transitions rejected; one-prediction-per-match).
- Assert raised domain events where relevant.

### Handlers
- Mock abstractions: `IApplicationDbContext`, `IClock`, `ICurrentUserService`.
- Cover the success path and **each** thrown domain exception (assert the `ErrorCodes`).
- Verify the handler delegates to the domain method (not re-implementing logic) and does **not** call `SaveChanges` itself.

### Validators
- Instantiate the validator, run valid and invalid commands, assert the failing property/message for each rule (ranges, required).

### Endpoints (integration)
- Use **`WebApplicationFactory`** + **SQLite/in-memory**; override `IClock`; stub the football-API twin.
- One test per declared response/problem code, plus an invalid request asserting `400` with the `errors` structure.

## 4. Run them
Run `dotnet test` for the affected project(s). Do not consider the task done while tests fail or the target is only partially covered.

## Output
Summarize: files added, the layers covered, factory methods added/reused, and the `dotnet test` result.
