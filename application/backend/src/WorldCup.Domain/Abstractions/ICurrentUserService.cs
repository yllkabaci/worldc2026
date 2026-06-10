namespace WorldCup.Domain.Abstractions;

/// <summary>The authenticated caller, surfaced to handlers without coupling them to HttpContext.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
}
