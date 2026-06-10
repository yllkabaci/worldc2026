using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorldCup.Domain.Exceptions;

namespace WorldCup.Api.Common.Errors;

/// <summary>Translates exceptions to RFC 7807 ProblemDetails. Typed domain exceptions map via their ErrorCodes.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, errorCode) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Handled {ErrorCode}", errorCode);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message
        };

        if (errorCode is not null)
        {
            problem.Extensions["errorCode"] = errorCode;
        }

        if (exception is ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int Status, string Title, string? ErrorCode) Map(Exception ex) => ex switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", ErrorCodes.ValidationError.ToString()),
        DomainException dex => (StatusFor(dex.Code), dex.Code.ToString(), dex.Code.ToString()),
        _ => (StatusCodes.Status500InternalServerError, "Server error", null)
    };

    private static int StatusFor(ErrorCodes code) => code switch
    {
        ErrorCodes.ValidationError => StatusCodes.Status400BadRequest,
        ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ErrorCodes.NotFound => StatusCodes.Status404NotFound,
        ErrorCodes.Conflict => StatusCodes.Status409Conflict,
        ErrorCodes.FootballApiUnavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };
}
