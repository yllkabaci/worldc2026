using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WorldCup.Api.Common.Modules;
using WorldCup.Api.Features.Authentication.Login;
using WorldCup.Api.Features.Authentication.Me;
using WorldCup.Api.Features.Authentication.Register;

namespace WorldCup.Api.Features.Authentication;

public sealed class AuthenticationModule : IFeatureModule, IEndpointModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // No feature-specific services; handlers/validators are auto-discovered.
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RegisterEndpoint.Map(app);
        LoginEndpoint.Map(app);
        MeEndpoint.Map(app);
    }
}
