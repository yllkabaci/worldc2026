using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WorldCup.Api.Common.Modules;

public static class ModuleExtensions
{
    public static IServiceCollection AddFeatureModules(this IServiceCollection services, Assembly assembly)
    {
        foreach (var module in Instantiate<IFeatureModule>(assembly))
        {
            module.ConfigureServices(services);
        }
        return services;
    }

    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        foreach (var module in Instantiate<IEndpointModule>(assembly))
        {
            module.MapEndpoints(app);
        }
        return app;
    }

    private static IEnumerable<T> Instantiate<T>(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => typeof(T).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (T)Activator.CreateInstance(t)!);
    }
}
