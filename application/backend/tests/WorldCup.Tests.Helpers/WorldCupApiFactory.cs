using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorldCup.Domain.Abstractions;
using WorldCup.Infrastructure.Persistence;

namespace WorldCup.Tests.Helpers;

/// <summary>Boots the API in-process backed by a SQLite in-memory database for integration tests.</summary>
public sealed class WorldCupApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    /// <summary>Controllable clock shared with the API host. Set Clock.UtcNow to drive time-dependent tests.</summary>
    public TestClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
