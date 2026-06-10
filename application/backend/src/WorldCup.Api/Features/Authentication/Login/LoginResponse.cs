namespace WorldCup.Api.Features.Authentication.Login;

public sealed record LoginResponse
{
    public required string Token { get; init; }
}
