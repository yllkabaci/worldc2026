---
paths:
  - "src/**/Persistence/**/*.cs"
  - "src/**/*DbContext*.cs"
  - "src/**/Configurations/**/*.cs"
  - "src/WorldCup.Infrastructure/**/*.cs"
---

# EF Core Persistence

How Entity Framework Core is configured. This is the binding spec for the persistence layer — an agent should be able to recreate it from this file alone.

## Provider, projects, and boundary
- **ORM:** EF Core. **Provider:** SQLite for the hackathon run; **SQL Server** is the prod target. Selected by the connection string only — no provider-specific code in handlers/domain.
- `ApplicationDbContext` lives in **`WorldCup.Infrastructure/Persistence`** and implements **`IApplicationDbContext`** (declared in `WorldCup.Domain/Abstractions`).
- The **Domain stays persistence-free**: no EF types in aggregates/value objects. Mapping is done in Infrastructure configurations, never with EF attributes on domain types.
- Handlers depend on `IApplicationDbContext`, never on `ApplicationDbContext` directly (see `mediatr-cqrs.md`, `handler-no-httpcontext.md`).

## IApplicationDbContext
Expose one `DbSet<TAggregateRoot>` per aggregate root plus `SaveChangesAsync`. Only **aggregate roots** get a `DbSet`; child entities/value objects are reached through their root.

```csharp
public interface IApplicationDbContext
{
    DbSet<Match> Matches { get; }
    DbSet<Prediction> Predictions { get; }
    DbSet<User> Users { get; }
    DbSet<ScoringRuleSet> ScoringRuleSets { get; }
    // ... one per aggregate root as features land
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```
(The skeleton ships with only `SaveChangesAsync`; add each `DbSet` with its feature.)

## ApplicationDbContext
```csharp
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Match> Matches => Set<Match>();
    // ... one expression-bodied DbSet per aggregate root

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```
- `sealed`, primary constructor, `Set<T>()`-backed DbSets.
- All mapping is discovered via `ApplyConfigurationsFromAssembly` — **no inline `modelBuilder.Entity<>()` in OnModelCreating**.
- No lazy-loading proxies. No `DbContext`-level query filters unless a rule calls for them.

## Registration (Infrastructure `AddInfrastructure`)
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(                       // UseSqlServer in prod
        connectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
```
- `DbContext` is **scoped**. Connection string comes from `ConnectionStrings:Default` (config/env/secret) — **never hardcoded** (see `configuration`).
- Migrations live in the **Infrastructure** assembly (set explicitly so the API startup project can host them).

## Entity configurations (one per aggregate)
One `IEntityTypeConfiguration<T>` per aggregate root in `Persistence/Configurations/{Aggregate}Configuration.cs`.

```csharp
public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> b)
    {
        b.ToTable("Matches");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).HasConversion(id => id.Value, v => new MatchId(v)); // strongly-typed id
        b.Property(m => m.Stage).HasConversion<string>().HasMaxLength(32);          // enum as string
        b.Property(m => m.KickoffUtc);
        b.OwnsOne(m => m.Result);                                                   // value object as owned type
        b.Property(m => m.ScoringRuleSetId).HasConversion(id => id.Value, v => new ScoringRuleSetId(v));
        b.HasIndex(m => m.KickoffUtc);
    }
}
```

Configuration rules:
- **Keys:** every aggregate has a key; configure it explicitly.
- **Strongly-typed IDs:** map with a `HasConversion` (id ↔ underlying value). Prefer a shared convention (below) so every id type is handled once. Reference other aggregates **by id** — no navigation properties across aggregate roots.
- **Value objects:** map as **owned types** (`OwnsOne`/`OwnsMany`) — e.g. `Score`, `PointsBreakdown`. Owned types are part of the parent's table unless explicitly split.
- **Enums:** stored **as strings** (`HasConversion<string>()`) with a sensible `HasMaxLength`, matching the JSON convention (`json-serialization.md`).
- **Money/points:** `decimal` columns with **explicit precision** (`HasPrecision(18, 2)` or as the domain requires). **Never** `double`/`float` (see `scoring-engine.md`).
- **Strings:** set `HasMaxLength`; mark `IsRequired()` to match domain invariants (validation still lives in the domain).
- **Timestamps:** store **UTC**. Use `DateTimeOffset` for instants; the app converts to local on the client.
- **Indexes:** add for common query/filter paths (e.g. match kickoff, leaderboard ordering, unique invite code, one-prediction-per-(match,user) unique index).
- **Concurrency:** add an optimistic concurrency token where concurrent writes matter (e.g. settlement / user totals) — a `rowversion`/`[Timestamp]`-style token (SQL Server) or a `Version` integer concurrency token (portable to SQLite).

## Strongly-typed ID convention (configure once)
Register a value converter for every strongly-typed id so individual configurations don't repeat it:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder builder)
{
    builder.Properties<MatchId>().HaveConversion<MatchIdConverter>();
    builder.Properties<UserId>().HaveConversion<UserIdConverter>();
    builder.Properties<PredictionId>().HaveConversion<PredictionIdConverter>();
    // ...one per id type
}
```
Each converter is a `ValueConverter<TId, Guid>` in `Persistence/Converters/`.

## Migrations
- **Design-time factory:** `ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>` (in `Persistence/`) builds the context for tooling so `dotnet ef` runs without booting the API.
- Create/extend migrations from the backend root:
  ```bash
  dotnet ef migrations add <Name> --project src/WorldCup.Infrastructure --startup-project src/WorldCup.Api
  ```
- **Every feature that adds/changes an entity adds a migration** in the same change. Migration names are descriptive (`AddPredictionAggregate`, not `Update1`).
- **Startup:** the API applies `Database.Migrate()` on boot, **skipped when the host environment is `Testing`** (tests manage their own database).
- **Do not** use `EnsureCreated()` in the application — it bypasses migrations. (`EnsureCreated` is allowed only in the in-memory test fixture; see `testing-conventions.md`.)

## Querying & writing
- **Queries are read-only:** use `AsNoTracking()` and project straight into response DTOs (see `mediatr-cqrs.md`). No tracking, no lazy loading.
- **Writes:** load the aggregate, invoke a domain method, and let the **UnitOfWork behavior** call `SaveChangesAsync` once — **handlers never call `SaveChanges`** themselves.
- **Loading within an aggregate:** use explicit `Include` only for owned/child data of the same aggregate; never traverse into another aggregate.
- **Domain events** are collected on aggregates and dispatched **after** `SaveChanges` by the UnitOfWork behavior (see `domain-model-ddd.md`).
- **No raw SQL.** Parameterised LINQ only. No string-built queries.

## Seeding
- Dev/demo seed data lives in `Persistence/SeedData.cs`, applied behind a development-only step (see the `/seed-demo` command). Prefer runtime seeding over `HasData` for realistic, related demo data. Real fixtures originate from the football-data.org provider (`02-matches.md`); seeding must not hardcode results the provider owns beyond the demo set.

## SQLite caveats (dev) vs SQL Server (prod)
- SQLite stores `decimal` as TEXT/REAL — rely on the configured converters and keep money as `decimal` in code; verify ordering/comparison behaves for leaderboard sorts.
- Some `ALTER` operations are limited in SQLite; EF handles most via table rebuilds. Keep schema parity in mind — prod is SQL Server.
- Connection string switches the provider; no other code changes.

## Forbidden
No EF attributes on domain types, no inline mapping in `OnModelCreating`, no `double`/`float` columns for money, no raw SQL, no `EnsureCreated()` in the app, no lazy-loading proxies, no cross-aggregate navigation properties, no hardcoded connection strings.
