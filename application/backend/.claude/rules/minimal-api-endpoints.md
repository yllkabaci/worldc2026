---
paths:
  - "src/**/Features/**/*Endpoint*.cs"
---

# Minimal API Endpoint Conventions

Endpoints are thin transport adapters: bind the request, map it to a MediatR command/query, send it, and wrap the result. **No business logic and no data access in the endpoint** (see `business-rule-placement.md`). **No MVC controllers.**

## Structure
A `public static class` with a single public `Map` method and a private `HandleAsync`.

```csharp
public static class MakePredictionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/matches/{matchId:guid}/prediction", HandleAsync)
            .WithName(RouteNames.MakePrediction)
            .WithSummary("Submit a prediction for a match")
            .WithDescription("Creates or replaces the caller's prediction while the match is open.")
            .RequireAuthorization("User")
            .Produces<ApiResponse<MakePredictionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid matchId,
        [FromBody] MakePredictionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(request.ToCommand(matchId), cancellationToken);
        return Results.Ok(response.ToApiResponse());
    }
}
```

## Key Conventions
- **Static class**: endpoints are stateless; no instance.
- **Map method**: single public method that registers the route.
- **HandleAsync**: private async method; binds, sends to MediatR, wraps the result.
- **Route names**: constants from a `RouteNames` class - never magic strings.
- **Metadata chain**: `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`, `.ProducesProblem()`, `.ProducesValidationProblem()`.
- **Authorization**: `.RequireAuthorization("<policy>")` on every protected endpoint (`User` / `Admin`).
- **Mediation**: inject `ISender` via `[FromServices]`; map the request DTO to a command/query (`request.ToCommand(...)`) and `Send` it. The endpoint never references a handler type directly.
- **CancellationToken**: always the last parameter with `= default`; it flows into `Send`.
- **No `HttpContext` into MediatR**: an endpoint may take `HttpContext` to set request tags, but must not pass it into the command/handler (see `handler-no-httpcontext.md`).

## Validation
Validation runs in the **MediatR `ValidationBehavior`** (see `fluent-validation.md`), not in the endpoint. The endpoint only declares `.ProducesValidationProblem()`; invalid requests are short-circuited into a `400` ProblemDetails before the handler executes.

## Response Wrapping
- **Success** payloads are wrapped in the `ApiResponse<T>` envelope via `response.ToApiResponse()` (lists via `.ToApiListResponse()`). Endpoints return `Results.Ok(...)` / `Results.Created(...)` with the envelope.
- **Failures** are emitted as RFC 7807 ProblemDetails (see `error-codes.md`) - never wrapped in `ApiResponse<T>`. Validation failures and unhandled exceptions are converted centrally; the endpoint does not build ProblemDetails by hand.

## Testing
Endpoints are thin wrappers and do not need exhaustive unit tests. Cover them with **integration tests** (primary coverage): one per `Produces<T>()` response, each `ProducesProblem()` status, and the validation path (send an invalid request, assert `400`). See `testing-conventions.md`.

## Module wiring
Endpoints do not self-register. The feature's `IEndpointModule` (in `{Feature}Module.cs`) calls each use case's `Endpoint.Map(app)`; the module itself is auto-discovered by assembly scan (see `vertical-slice-architecture.md`). There is no manual endpoint wiring in `Program.cs`.
