using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorldCup.Api.Common.Http;
using WorldCup.Api.Common.Routing;

namespace WorldCup.Api.Features.Authentication.Register;

public static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", HandleAsync)
            .WithName(RouteNames.Register)
            .WithSummary("Register a new user")
            .AllowAnonymous()
            .Produces<ApiResponse<RegisterResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] RegisterRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(request.ToCommand(), cancellationToken);
        return Results.Ok(response.ToApiResponse());
    }
}
