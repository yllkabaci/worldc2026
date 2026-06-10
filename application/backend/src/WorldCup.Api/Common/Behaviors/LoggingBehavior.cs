using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WorldCup.Api.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Handling {RequestName}", name);
        try
        {
            var response = await next();
            logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{RequestName} failed after {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
