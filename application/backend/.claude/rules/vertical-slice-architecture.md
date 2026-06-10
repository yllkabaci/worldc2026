---
paths:
  - "src/**/Features/**/*.cs"
---

# Vertical Slice Architecture

Features are self-contained slices. Each slice owns its **Endpoint, Command/Query, Handler, Validator, and Response DTO**, co-located in one folder inside the API project. Mediation is via **MediatR** (see `mediatr-cqrs.md`).

## Auto-Discovery
Slices register themselves via `IFeatureModule` (DI services) and `IEndpointModule` (route mapping) interfaces, discovered at startup by assembly scanning. **No manual registration** in `Program.cs` when adding a feature.

## Feature Slice Structure
```
Features/
  {FeatureName}/
    {FeatureName}Module.cs            # Implements IFeatureModule, IEndpointModule
    {UseCase}/                        # e.g. MakePrediction, GetLeaderboard
      {UseCase}Endpoint.cs            # Minimal API endpoint (see minimal-api-endpoints.md)
      {UseCase}Command.cs             # or {UseCase}Query.cs - MediatR IRequest<TResponse>
      {UseCase}Handler.cs             # MediatR IRequestHandler<TRequest, TResponse>
      {UseCase}Validator.cs           # FluentValidation AbstractValidator
      {UseCase}Response.cs            # response DTO (sealed record)
```

Use cases sit **directly** under the feature name. Do **not** add intermediate grouping directories (`Endpoints/`, `Handlers/`, `Queries/`, `Commands/`) between the feature and the use-case folder.

## What lives where
- **API project (`WorldCup.Api`)** - hosts all feature slices (endpoint + command/query + handler + validator + response). This is the only place slices live.
- **Domain (`WorldCup.Domain`)** - aggregates, value objects, domain events, invariants. No framework or persistence dependencies (see `domain-model-ddd.md`).
- **Infrastructure (`WorldCup.Infrastructure`)** - EF Core `DbContext`, persistence, external clients, `IClock`. Implements abstractions the handlers depend on.

## Dependency Direction
- `Api` depends on `Infrastructure` and `Domain`.
- `Infrastructure` depends on `Domain`.
- `Domain` has no dependencies on other project layers.
- Handlers depend on **abstractions** (`IApplicationDbContext`, `IClock`, external-API interfaces), never concrete infrastructure types.

## Adding a New Feature
1. Create `src/WorldCup.Api/Features/{FeatureName}/`.
2. Add `{FeatureName}Module.cs` implementing `IFeatureModule` (register DI) and `IEndpointModule` (map routes).
3. Add a `{UseCase}/` folder per use case with Endpoint, Command/Query, Handler, Validator, Response.
4. The handler talks to the domain and to abstractions only - no HTTP types (see `handler-no-httpcontext.md`), no business logic in the endpoint (see `business-rule-placement.md`).
