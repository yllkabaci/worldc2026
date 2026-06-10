using Microsoft.EntityFrameworkCore;
using WorldCup.Domain.Users;

namespace WorldCup.Domain.Abstractions;

/// <summary>
/// Persistence boundary used by handlers. One DbSet per aggregate root, added as features land.
/// The UnitOfWork behavior is the only caller of SaveChangesAsync.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
