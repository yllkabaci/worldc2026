---
paths:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---

# Async and CancellationToken Conventions

## CancellationToken
- ALWAYS accept `CancellationToken` as the **last parameter**, with `= default` on public entry points.
- ALWAYS name it **`cancellationToken`** (not `ct`, `token`, or other abbreviations).
- ALWAYS propagate it through the entire async chain: endpoint -> `ISender.Send` -> handler -> EF Core / external `HttpClient`.

```csharp
public async Task<MakePredictionResponse> Handle(
    MakePredictionCommand command,
    CancellationToken cancellationToken)
{
    var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == command.MatchId, cancellationToken);
    // ...
    return new MakePredictionResponse(/* ... */);
}
```

## Async all the way
- No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` - never block on async.
- If a method calls async code, it is itself async and returns `Task<T>` / `Task`.

## ConfigureAwait
- Do **NOT** use `ConfigureAwait(false)`. ASP.NET Core has no `SynchronizationContext`, so it is unnecessary noise.

## ValueTask
- Return **`ValueTask<T>`** only for hot, frequently-synchronous abstractions (e.g. cache or `IClock` lookups) where allocation matters.
- Default to `Task<T>` everywhere else; do not await a `ValueTask` more than once.

## Queries
- Read query handlers use `AsNoTracking()` and pass `cancellationToken` into the EF query (see `mediatr-cqrs.md`).
