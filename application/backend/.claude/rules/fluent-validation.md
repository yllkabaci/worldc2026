---
paths:
  - "src/**/Features/**/*Validator*.cs"
---

# FluentValidation Conventions

Commands and queries are validated with FluentValidation, run inside the **MediatR `ValidationBehavior`** (see `mediatr-cqrs.md`) - not via an endpoint filter. Validators check **input shape only**; business invariants live in the domain (see `business-rule-placement.md`).

## Validator Structure
Validators target the **command/query** (the MediatR request), co-located in the slice.

```csharp
public sealed class MakePredictionValidator : AbstractValidator<MakePredictionCommand>
{
    public MakePredictionValidator()
    {
        RuleFor(x => x.HomeGoals)
            .InclusiveBetween(0, 20);   // BR-010: non-negative, max 20

        RuleFor(x => x.AwayGoals)
            .InclusiveBetween(0, 20);
    }
}
```

## Key Patterns
- **Sealed class** inheriting `AbstractValidator<TCommand>` / `AbstractValidator<TQuery>`.
- **Constructor-based** rules (never method overrides).
- **`Cascade(CascadeMode.Stop)`** on fields with multiple rules.
- **Conditional**: `.When()` for optional/context-dependent fields.
- **Collections**: `RuleForEach()`.
- **Ranges mirror the business doc** (input bounds only): goals `0-20` (BR-010), yellow cards `0-20` (BR-024), red cards `0-10` (BR-025), substitutions `0-5` per team (BR-028).
- **Allowed values**: `Must()` with static arrays for flexibility where an enum would be too rigid.

## What does NOT belong in a validator
Stateful / cross-aggregate / business rules - "deadline passed", "match exists", "one prediction per match", scoring. These have no transaction or loaded aggregate here and can race; enforce them in the domain method, which throws a typed exception (see `error-codes.md`).

## Naming, Location, Discovery
- **Naming**: `{UseCase}Validator`.
- **Location**: co-located in the feature slice, next to the Endpoint, Command, and Handler.
- **Auto-discovery**: registered via assembly scanning - no manual DI registration.

## Failure behavior
The `ValidationBehavior` aggregates failures and short-circuits to a **RFC 7807 ProblemDetails** with HTTP `400` and the `errors` dictionary (field -> messages) before the handler runs. Endpoints declare `.ProducesValidationProblem()`.
