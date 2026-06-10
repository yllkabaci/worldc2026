using System.Net.Http.Json;
using System.Text.Json;
using WorldCup.Domain.Abstractions;

namespace WorldCup.Infrastructure.ExternalApis;

/// <summary>
/// football-data.org v4 client. Auth via the 'X-Auth-Token' header (set on the injected HttpClient).
/// Endpoint: GET competitions/{code}/matches. Free tier is rate-limited (~10 req/min) — callers should cache/poll sparingly.
/// </summary>
public sealed class FootballDataOrgClient(HttpClient http, FootballDataOptions options) : IFootballApi
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<ProviderFixture>> GetFixturesAsync(CancellationToken cancellationToken = default)
    {
        var payload = await http.GetFromJsonAsync<MatchesResponse>(
            $"competitions/{options.CompetitionCode}/matches", Json, cancellationToken);

        if (payload?.Matches is null)
        {
            return [];
        }

        return payload.Matches.Select(m => new ProviderFixture(
            ExternalId: m.Id.ToString(),
            Stage: m.Stage ?? "UNKNOWN",
            Group: m.Group,
            HomeTeam: m.HomeTeam?.Name,
            AwayTeam: m.AwayTeam?.Name,
            KickoffUtc: m.UtcDate,
            Status: m.Status ?? "SCHEDULED")).ToList();
    }

    public async Task<ProviderMatchResult?> GetResultAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var m = await http.GetFromJsonAsync<MatchDto>($"matches/{externalId}", Json, cancellationToken);
        if (m is null || !string.Equals(m.Status, "FINISHED", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // NOTE: scoring uses the REGULATION (90-minute) result. 'fullTime' is used here as the recorded
        // result; the exact regulation-vs-extra-time field mapping MUST be verified against a live
        // knockout sample before relying on it (see specs/features/02-matches.md, open verification item).
        var home = m.Score?.FullTime?.Home ?? 0;
        var away = m.Score?.FullTime?.Away ?? 0;
        return new ProviderMatchResult(m.Id.ToString(), home, away, m.Status!);
    }

    // --- minimal wire models for football-data.org v4 (not domain types) ---
    private sealed record MatchesResponse(List<MatchDto>? Matches);
    private sealed record MatchDto(int Id, DateTimeOffset UtcDate, string? Status, string? Stage, string? Group,
        TeamDto? HomeTeam, TeamDto? AwayTeam, ScoreDto? Score);
    private sealed record TeamDto(int? Id, string? Name);
    private sealed record ScoreDto(string? Winner, string? Duration, ScoreLineDto? FullTime, ScoreLineDto? HalfTime);
    private sealed record ScoreLineDto(int? Home, int? Away);
}
