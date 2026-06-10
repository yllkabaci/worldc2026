using Microsoft.EntityFrameworkCore;
using WorldCup.Domain.Abstractions;

namespace WorldCup.Infrastructure.Persistence;

/// <summary>EF Core context implementing the persistence boundary. Entity configurations are discovered from this assembly as features add them.</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
