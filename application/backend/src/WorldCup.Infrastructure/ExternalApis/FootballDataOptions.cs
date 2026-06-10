namespace WorldCup.Infrastructure.ExternalApis;

/// <summary>Configuration for the football-data.org provider (v4). The token is a secret; never commit it.</summary>
public sealed record FootballDataOptions
{
    public string BaseUrl { get; init; } = "https://api.football-data.org/v4/";
    public string ApiToken { get; init; } = "";
    /// <summary>football-data.org competition code. World Cup = "WC".</summary>
    public string CompetitionCode { get; init; } = "WC";
}
