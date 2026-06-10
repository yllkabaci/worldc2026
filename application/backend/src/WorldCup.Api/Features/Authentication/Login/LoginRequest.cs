namespace WorldCup.Api.Features.Authentication.Login;

/// <summary>Authenticates a user and returns a JWT.</summary>
public sealed record LoginRequest
{
    /// <summary>Email address.</summary>
    /// <example>jane@example.com</example>
    public required string Email { get; init; }

    /// <summary>Password.</summary>
    /// <example>Password1!</example>
    public required string Password { get; init; }

    public LoginCommand ToCommand() => new(Email, Password);
}
