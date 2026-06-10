namespace WorldCup.Api.Features.Authentication.Me;

public sealed record MeResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
