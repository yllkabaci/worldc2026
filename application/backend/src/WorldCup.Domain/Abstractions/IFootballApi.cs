namespace WorldCup.Domain.Abstractions;

/// <summary>
/// Fixture/result provider boundary. The production implementation reads from football-data.org (v4);
/// a deterministic twin implements it for local/dev and tests. Handlers/domain depend only on this abstraction.
/// </summary>
public interface IFootballApi
{
    /// <summary>All fixtures for the configured competition (World Cup 2026).</summary>
    Task<IReadOnlyList<ProviderFixture>> GetFixturesAsync(CancellationToken cancellationToken = default);

    /// <summary>The result for one fixture, or null if not yet finished / unknown.</summary>
    Task<ProviderMatchResult?> GetResultAsync(string externalId, CancellationToken cancellationToken = default);
}

/// <summary>A fixture as supplied by the provider. Teams may be null until a knockout slot is determined.</summary>
public sealed record ProviderFixture(
    string ExternalId,
    string Stage,
    string? Group,
    string? HomeTeam,
    string? AwayTeam,
    DateTimeOffset KickoffUtc,
    string Status);

/// <summary>
/// A recorded result. Goals are the REGULATION (90-minute) score used for our scoring rules
/// (see scoring-engine.md). Extra time / shootouts are excluded from these figures.
/// </summary>
public sealed record ProviderMatchResult(
    string ExternalId,
    int RegulationHomeGoals,
    int RegulationAwayGoals,
    string Status);
