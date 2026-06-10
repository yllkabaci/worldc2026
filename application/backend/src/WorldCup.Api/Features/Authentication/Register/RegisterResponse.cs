namespace WorldCup.Api.Features.Authentication.Register;

public sealed record RegisterResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}
