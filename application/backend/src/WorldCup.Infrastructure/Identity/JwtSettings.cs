namespace WorldCup.Infrastructure.Identity;

/// <summary>JWT signing configuration. For the hackathon a symmetric dev signing key is used.</summary>
public sealed record JwtSettings
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
    public int ExpiryMinutes { get; init; } = 60 * 24;
}
