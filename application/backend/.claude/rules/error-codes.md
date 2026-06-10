---
paths:
  - "src/**/ErrorHandling/**/*.cs"
  - "src/**/ErrorCodes*.cs"
  - "src/**/Domain/**/*Exception*.cs"
---

# Error Handling & Error Codes

Failures are signalled with **typed domain exceptions** carrying an `ErrorCodes` value. A single global `IExceptionHandler` maps them to **RFC 7807 ProblemDetails**. Handlers and the domain throw; they never build ProblemDetails by hand. (We use exceptions, not an Outcome/Result type - consistent with the MediatR/CQRS decision.)

## Error code enum
```csharp
public enum ErrorCodes
{
    // 0000-0099 API errors
    ValidationError,        // WC-0001
    NotFound,               // WC-0002
    Conflict,               // WC-0003
    Unauthorized,           // WC-0004
    Forbidden,              // WC-0005

    // 1000-1999 External service errors
    FootballApiUnavailable, // WC-1001

    // 5000-5999 Feature/business-rule violations
    MatchNotFound,             // WC-5001
    PredictionWindowClosed,    // WC-5002
    DuplicatePrediction,       // WC-5003
    MatchNotSettleable,        // WC-5004
}
```

- Format **`WC-NNNN`**, ranges reserved by concern (API `0000-0099`, infrastructure `0100-0199`, external `1000-1999`, business `5000-5999`).
- New codes take the next free number in the right range.

## Typed exceptions
```csharp
public abstract class DomainException(ErrorCodes code, string message) : Exception(message)
{
    public ErrorCodes Code { get; } = code;
}

public sealed class PredictionWindowClosedException(MatchId matchId)
    : DomainException(ErrorCodes.PredictionWindowClosed,
        $"Prediction window is closed for match {matchId.Value}");
```

- The domain/handlers throw these (see `domain-model-ddd.md`, `business-rule-placement.md`).
- Use specific codes - prefer `MatchNotFound` over a generic `NotFound` when a specific code exists.

## Central code -> HTTP status map
One mapping table (in the exception handler) translates each `ErrorCodes` to an HTTP status:

| Code group | HTTP |
|------------|------|
| ValidationError | 400 |
| Unauthorized | 401 |
| Forbidden | 403 |
| NotFound / MatchNotFound | 404 |
| Conflict / DuplicatePrediction / PredictionWindowClosed / MatchNotSettleable | 409 |
| FootballApiUnavailable | 503 |
| (unmapped / unexpected) | 500 |

The `GlobalExceptionHandler` (registered via `AddExceptionHandler` + `UseExceptionHandler`) produces `Results.Problem(...)` with `type`/`title`/`status`/`detail` and the `ErrorCodes` name in an extension member. **Validation** failures come from the MediatR `ValidationBehavior` as a `400` ProblemDetails with the `errors` dictionary (see `fluent-validation.md`).

## Rules
- Never leak stack traces or internal details to clients; log them server-side with structured properties.
- Every distinct failure mode a caller would handle differently gets its own code.
- Do not reuse one code for semantically different failures (e.g. "match missing" vs "match not yet settleable").
