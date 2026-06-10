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
    /// <summary>Registers persistence, time, identity, and the fixture/result provider. Config values come from the API composition root.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string sqliteConnectionString,
        JwtSettings jwtSettings,
        FootballDataOptions footballDataOptions)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                sqliteConnectionString,
                sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IJwtIssuer, JwtIssuer>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Real football-data.org client when an API token is configured; deterministic twin otherwise (dev/test).
        if (string.IsNullOrWhiteSpace(footballDataOptions.ApiToken))
        {
            services.AddSingleton<IFootballApi, FootballApiTwin>();
        }
        else
        {
            services.AddSingleton(footballDataOptions);
            services.AddHttpClient<IFootballApi, FootballDataOrgClient>(client =>
            {
                client.BaseAddress = new Uri(footballDataOptions.BaseUrl);
                client.DefaultRequestHeaders.Add("X-Auth-Token", footballDataOptions.ApiToken);
            });
        }

        return services;
    }
}
