using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WorldCup.Api.Common.Modules;

/// <summary>A feature registers its DI services here.</summary>
public interface IFeatureModule
{
    void ConfigureServices(IServiceCollection services);
}

/// <summary>A feature maps its endpoints here (calls each use case's Endpoint.Map).</summary>
public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
