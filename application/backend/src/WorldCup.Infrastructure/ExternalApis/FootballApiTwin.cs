using WorldCup.Domain.Abstractions;

namespace WorldCup.Infrastructure.ExternalApis;

/// <summary>Deterministic in-process stand-in for football-data.org. Used in non-prod and tests (no live calls). Returns no fixtures by default; seed data drives demos.</summary>
public sealed class FootballApiTwin : IFootballApi
{
    public Task<IReadOnlyList<ProviderFixture>> GetFixturesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProviderFixture>>([]);

    public Task<ProviderMatchResult?> GetResultAsync(string externalId, CancellationToken cancellationToken = default) =>
        Task.FromResult<ProviderMatchResult?>(null);
}
