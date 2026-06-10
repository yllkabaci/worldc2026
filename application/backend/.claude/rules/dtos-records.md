---
paths:
  - "src/**/Features/**/*Request*.cs"
  - "src/**/Features/**/*Response*.cs"
  - "src/**/Features/**/*Command*.cs"
  - "src/**/Features/**/*Query*.cs"
---

# DTO and Record Conventions

## Request and Response DTOs
ALWAYS use `public sealed record` with `required` properties and `{ get; init; }`.

```csharp
/// <summary>Submits a prediction for a match.</summary>
public sealed record MakePredictionRequest
{
    /// <summary>Predicted goals for the home team.</summary>
    /// <example>2</example>
    public required int HomeGoals { get; init; }

    /// <summary>Predicted goals for the away team.</summary>
    /// <example>1</example>
    public required int AwayGoals { get; init; }
}
```

## Rules
- **`sealed record`**: all DTOs are sealed records, never classes.
- **`required`**: mandatory properties use the `required` keyword.
- **`{ get; init; }`**: properties are init-only (immutable after creation).
- **Nullable**: optional properties use `?`.
- **No methods / no logic**: records are pure data containers. A small `ToCommand(...)` mapping extension is allowed alongside the request (used by the endpoint), but no business logic.
- **XML docs**: `<summary>` on the record and each property; `<example>` with realistic sample values (drives OpenAPI, which the React app consumes).
- **Naming**: `{UseCase}Request`, `{UseCase}Response`, `{UseCase}ListResponse`, `{UseCase}Command`, `{UseCase}Query`.

## Location
- Request/Response/Command/Query records are **co-located in their feature slice** (see `vertical-slice-architecture.md`) - no separate Contracts project.
- Commands/queries are also records and follow the same rules (see `mediatr-cqrs.md`).

## Entities are not DTOs
- **Never** expose a domain entity or value object across the API boundary. Handlers project aggregates into response records.
- Use **strongly-typed IDs** (`MatchId`, `UserId`) in commands/queries; serialize them to their underlying value in responses where a client needs the raw id.

## Envelope
- Response records are the payload **inside** the `ApiResponse<T>` envelope returned by endpoints (see `minimal-api-endpoints.md`); the record itself does not include envelope fields.
