namespace WorldCup.Domain.Abstractions;

/// <summary>Issues signed JWTs for authenticated users.</summary>
public interface IJwtIssuer
{
    string IssueToken(Guid userId, string email, IReadOnlyCollection<string> roles);
}
