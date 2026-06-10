using WorldCup.Domain.Abstractions;

namespace WorldCup.Infrastructure.ExternalApis;

/// <summary>Deterministic in-process stand-in for the external football provider. Used in non-prod and tests.</summary>
public sealed class FootballApiTwin : IFootballApi
{
}
