using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorldCup.Api.Common.Http;
using WorldCup.Api.Common.Routing;

namespace WorldCup.Api.Features.Authentication.Login;

public static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", HandleAsync)
            .WithName(RouteNames.Login)
            .WithSummary("Log in and receive a JWT")
            .AllowAnonymous()
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(request.ToCommand(), cancellationToken);
        return Results.Ok(response.ToApiResponse());
    }
}
