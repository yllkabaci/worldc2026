namespace WorldCup.Api.Features.Authentication.Register;

/// <summary>Registers a new user.</summary>
public sealed record RegisterRequest
{
    /// <summary>Email address.</summary>
    /// <example>jane@example.com</example>
    public required string Email { get; init; }

    /// <summary>Password (min 8 chars, 1 digit, 1 uppercase, 1 special).</summary>
    /// <example>Password1!</example>
    public required string Password { get; init; }

    public RegisterCommand ToCommand() => new(Email, Password);
}
