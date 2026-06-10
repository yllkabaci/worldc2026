---
paths:
  - "src/**/Features/**/*Handler*.cs"
  - "src/**/Features/**/*Command*.cs"
  - "src/**/Features/**/*Query*.cs"
---

# Handlers Must Not Depend on HttpContext

MediatR handlers (`IRequestHandler<,>`) and their commands/queries must **NEVER** accept `HttpContext` or `IHttpContextAccessor`. A feature can be triggered by an API endpoint today and by an event consumer or background worker tomorrow - `HttpContext` does not exist there. The endpoint is the only place that touches HTTP.

## Current user & request identity

Handlers access the current user through an injected **`ICurrentUserService`** abstraction (backed by `IHttpContextAccessor` in the API, swappable in other hosts). Do **not** read claims from `HttpContext` inside a handler, and do **not** thread a `UserId` through every command for identity alone.

```csharp
// Bad - handler coupled to the HTTP pipeline
public sealed class MakePredictionHandler(IApplicationDbContext db)
    : IRequestHandler<MakePredictionCommand, MakePredictionResponse>
{
    public Task<MakePredictionResponse> Handle(MakePredictionCommand cmd, CancellationToken cancellationToken)
    {
        var userId = /* httpContext.User.FindFirst("sub") */; // NO
    }
}

// Good - identity via injected abstraction
public sealed class MakePredictionHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IClock clock)
    : IRequestHandler<MakePredictionCommand, MakePredictionResponse>
{
    public async Task<MakePredictionResponse> Handle(MakePredictionCommand cmd, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        // ...
    }
}
```

`ICurrentUserService` exposes `UserId`, `Email`, `Roles`, and `IsAuthenticated`. It is registered in the API host; non-HTTP hosts provide their own implementation.

## Observability context

Enrich logs/traces with a logging scope inside the handler (AsyncLocal-based, works in any host) - not via HTTP request tags:

```csharp
using var scope = logger.BeginScope(new Dictionary<string, object> { ["MatchId"] = cmd.MatchId });
```

## Summary

| Need | Solution |
|------|----------|
| Current user identity / claims | `ICurrentUserService` (injected) |
| Current time | `IClock` (injected) - never `DateTime.UtcNow` in a handler |
| Add log/trace context | `logger.BeginScope(...)` |
| Arbitrary request data (headers) | Extract at the endpoint, pass as a typed command property |
