---
name: create-feature
description: Use when adding a new backend feature or use case to the World Cup 2026 prediction API, given either a free-text description or a path to a specification .md file. Scaffolds a complete vertical slice (endpoint, command/query, handler, validator, response, and feature module) that conforms to every rule in .claude/rules. Triggers - "new feature", "add an endpoint", "implement this use case", "build the X feature", "create a slice", "scaffold a feature from this spec".
---

# Create Feature

Scaffold a new vertical slice for the backend that obeys the project rules. Input is **either** a free-text feature description **or** a path to a specification `.md` file.

## 0. Load the rules (mandatory)
Read these before writing anything; they are the contract:
`.claude/rules/vertical-slice-architecture.md`, `minimal-api-endpoints.md`, `mediatr-cqrs.md`,
`handler-no-httpcontext.md`, `business-rule-placement.md`, `domain-model-ddd.md`,
`dtos-records.md`, `fluent-validation.md`, `error-codes.md`, `json-serialization.md`,
`async-patterns.md`. If the feature touches scoring, also read `scoring-engine.md`.

## 1. Understand the input
- **If given a `.md` spec**: read the whole file. Extract the feature name, the use case(s), the observable behavior, inputs/outputs, business rules, edge/error conditions, and the auth/role required.
- **If given a description**: restate the use case in one sentence and list the inputs, the output, and the rules it must enforce.
- Cross-check against `WorldCup2026 BusinessLogic EN.docx` for the authoritative domain rules (point values, deadlines, ranges, void cases).

## 2. Plan the slice (decide, then confirm)
Produce a short plan and resolve every choice:
- **Feature & use case names** -> folder `Features/{FeatureName}/{UseCase}/`.
- **Command or Query?** Writes -> `ICommand<T>`; reads -> `IQuery<T>` (see `mediatr-cqrs.md`).
- **Route & verb**, and **auth policy** (`User` / `Admin` / `SuperAdmin`).
- **Aggregate(s) involved** and which **invariants** the domain must enforce (NOT the validator).
- **Request/Response DTO** shapes.
- **Input-shape validation** rules (ranges/required only) for the validator.
- **Error codes** needed (reuse existing in `error-codes.md`; add new `WC-NNNN` in the right range if a caller would handle it distinctly).

## 3. Spec-to-architecture gate (do not guess)
If any behavior, rule value, or boundary is ambiguous or missing, **HALT and ask** before generating code. Guessing is a failure. Only proceed once the plan is unambiguous.

## 4. Generate the slice
Create these files in `src/WorldCup.Api/Features/{FeatureName}/{UseCase}/`:

- `{UseCase}Command.cs` / `{UseCase}Query.cs` - `sealed record` implementing `ICommand<TResponse>` / `IQuery<TResponse>`. No `HttpContext`.
- `{UseCase}Response.cs` - `sealed record`, `required` + `init`, XML `<summary>`/`<example>` docs.
- `{UseCase}Request.cs` (if the HTTP body differs from the command) + a `ToCommand(...)` mapping.
- `{UseCase}Validator.cs` - `AbstractValidator<{UseCase}Command>`, **input shape only**.
- `{UseCase}Handler.cs` - `ICommandHandler`/`IQueryHandler`. Depends on abstractions (`IApplicationDbContext`, `IClock`, `ICurrentUserService`). Orchestrates: load aggregate -> invoke domain method -> map response. **No** `SaveChanges` (UnitOfWork behavior commits), **no** business logic, **no** HTTP types. Queries use `AsNoTracking()` and project to the DTO.
- `{UseCase}Endpoint.cs` - static class, `Map` + private `HandleAsync`: bind -> `ISender.Send(request.ToCommand(...))` -> `Results.Ok(response.ToApiResponse())`. Full metadata chain, `RequireAuthorization`, `Produces`/`ProducesProblem`/`ProducesValidationProblem`, `CancellationToken cancellationToken = default` last.

If the feature folder is new, also create `{FeatureName}Module.cs` implementing `IFeatureModule` (DI) and `IEndpointModule` (route mapping) so it is auto-discovered - no manual `Program.cs` edits.

Domain changes (new invariants, value objects, events) go in `WorldCup.Domain` per `domain-model-ddd.md`; use strongly-typed IDs and throw typed domain exceptions for violations.

## 5. Conformance checklist (verify each)
- Slice co-located, use case directly under the feature (no `Commands/`/`Handlers/` grouping).
- Command/query marker interfaces used; handler depends only on abstractions; no `HttpContext`.
- Validator = input shape only; invariants live in the domain.
- Success wrapped in `ApiResponse<T>`; failures via typed exceptions -> RFC 7807.
- `cancellationToken` named in full and propagated end to end.
- Enums serialize as strings; shared JSON defaults.

## 6. Then test it
After the production code compiles, invoke the **write-unit-tests** skill to cover the new slice (scoring/domain exhaustively, handler paths, validator rules, endpoint integration).

## Output
Summarize: files created, command vs query, route + auth, domain methods touched, any new error codes, and any assumptions made. List anything you halted on in step 3.
