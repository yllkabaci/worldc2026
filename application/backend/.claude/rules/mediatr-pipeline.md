---
paths:
  - "src/**/Behaviors/**/*.cs"
  - "src/**/*Behavior*.cs"
  - "src/**/*PipelineBehavior*.cs"
---

# MediatR Pipeline

How requests flow through MediatR. Complements `mediatr-cqrs.md` (markers/handlers); this file is the binding spec for the **behaviors** and is reconstructable on its own.

## Flow & order
Every `ISender.Send(request)` passes through pipeline behaviors before reaching the handler, in this fixed order (outermost first):

```
ISender.Send(command/query)
  → LoggingBehavior          (logs name + outcome + elapsed)
  → ValidationBehavior       (FluentValidation; 400 ProblemDetails on failure)
  → UnitOfWorkBehavior       (commands only: SaveChanges once + dispatch domain events)
  → Handler                  (orchestrates: load aggregate → domain method → map response)
```

Order is determined by **registration order**, which equals execution order (first registered = outermost). Do not reorder without reason.

## Registration (`Program`)
```csharp
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```
- Behaviors are **open generics** (`<,>`), transient, in the order above. Handlers and validators are discovered by assembly scan.
- `CancellationToken` is threaded through `next()` to the handler and onward (see `async-patterns.md`).

## LoggingBehavior
- Logs the request type name on entry, and the outcome + elapsed ms on exit (structured properties, never string interpolation — see `json-serialization.md`/observability). Re-throws on exception after logging a warning. No business logic.

## ValidationBehavior
- Resolves `IEnumerable<IValidator<TRequest>>`; if any exist, runs them and aggregates failures. On failure throws `FluentValidation.ValidationException`, which the global handler converts to a **`400` ProblemDetails** with an `errors` dictionary (field → messages). Applies to **both** commands and queries. Validators check **input shape only** (see `fluent-validation.md`, `business-rule-placement.md`).

## UnitOfWorkBehavior
- Runs only for **`ICommand<>`** requests (queries bypass it). After the handler returns successfully it calls `IApplicationDbContext.SaveChangesAsync(cancellationToken)` **exactly once**, then **dispatches domain events** (below). A command therefore = one commit; **handlers never call `SaveChanges`** themselves (see `mediatr-cqrs.md`, `ef-core-persistence.md`).
- If the handler throws, no commit happens and the exception propagates to the global handler.

## Domain-event dispatch
- Aggregates raise pure `IDomainEvent`s (Domain stays framework-free — see `domain-model-ddd.md`). After `SaveChangesAsync`, the UnitOfWork behavior collects events from tracked aggregates, **adapts** each to a MediatR `INotification`, publishes them, and clears the aggregates' event buffers.
- Dispatch is **after** persistence so handlers/notification-handlers observe committed state. Notification handlers must be idempotent and must not assume ordering across aggregates.

## Adding a behavior
- Implement `IPipelineBehavior<TRequest, TResponse>`, keep it generic and cross-cutting (no feature logic), and register it in the correct position (outer for logging/tracing, inner-but-before-handler for transaction concerns). Document why if it changes ordering.

## Forbidden
No business logic in behaviors; no `SaveChanges` in handlers (only the UnitOfWork behavior commits); no swallowing exceptions in a behavior; no dispatching domain events before commit; no per-request `new`-ing of `JsonSerializerOptions`/validators (resolve via DI).
