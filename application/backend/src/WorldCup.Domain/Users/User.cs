using WorldCup.Domain.Common;

namespace WorldCup.Domain.Users;

/// <summary>A registered participant. Created via <see cref="Register"/>; always starts Active and non-admin.</summary>
public sealed class User : AggregateRoot<UserId>
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsAdmin { get; private set; }
    public AccountStatus Status { get; private set; }

    private User() { } // EF

    private User(UserId id, string email, string passwordHash, bool isAdmin, AccountStatus status)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
        Status = status;
    }

    public bool IsActive => Status == AccountStatus.Active;

    /// <summary>Blocks the account (admin action). Blocked users cannot authenticate (BR-009).</summary>
    public void Block() => Status = AccountStatus.Blocked;

    /// <summary>Re-activates a blocked account.</summary>
    public void Activate() => Status = AccountStatus.Active;

    /// <summary>Grants admin rights (e.g. an existing admin promoting a user).</summary>
    public void GrantAdmin() => IsAdmin = true;

    /// <summary>Revokes admin rights.</summary>
    public void RevokeAdmin() => IsAdmin = false;

    public static User Register(string email, string passwordHash, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User(UserId.New(), email.Trim().ToLowerInvariant(), passwordHash, isAdmin: false, AccountStatus.Active);
        user.Raise(new UserRegisteredEvent(user.Id, nowUtc));
        return user;
    }
}
