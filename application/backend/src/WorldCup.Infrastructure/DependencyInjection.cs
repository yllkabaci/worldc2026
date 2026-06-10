using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldCup.Domain.Abstractions;
using WorldCup.Infrastructure.ExternalApis;
using WorldCup.Infrastructure.Identity;
using WorldCup.Infrastructure.Persistence;
using WorldCup.Infrastructure.Time;

namespace WorldCup.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence, time, identity, and the external-provider twin. Configuration values are passed in by the API composition root.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string sqliteConnectionString,
        JwtSettings jwtSettings)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IJwtIssuer, JwtIssuer>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IFootballApi, FootballApiTwin>();

        return services;
    }
}
