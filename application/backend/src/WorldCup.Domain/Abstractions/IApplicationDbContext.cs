namespace WorldCup.Domain.Abstractions;

/// <summary>
/// Persistence boundary used by handlers. Feature DbSets are added here as aggregates are introduced.
/// Kept minimal in the skeleton (zero features). The UnitOfWork behavior is the only caller of SaveChangesAsync.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
