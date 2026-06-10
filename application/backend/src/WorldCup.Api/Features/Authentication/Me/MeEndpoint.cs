using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorldCup.Api.Common.Http;
using WorldCup.Api.Common.Routing;

namespace WorldCup.Api.Features.Authentication.Me;

public static class MeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", HandleAsync)
            .WithName(RouteNames.Me)
            .WithSummary("Returns the authenticated user's identity")
            .RequireAuthorization("User")
            .Produces<ApiResponse<MeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(new GetMeQuery(), cancellationToken);
        return Results.Ok(response.ToApiResponse());
    }
}
