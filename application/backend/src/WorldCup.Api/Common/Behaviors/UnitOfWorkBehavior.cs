using MediatR;
using WorldCup.Api.Common.Cqrs;
using WorldCup.Domain.Abstractions;

namespace WorldCup.Api.Common.Behaviors;

/// <summary>Commits once after a successful command. Queries bypass the commit. (Domain-event dispatch hooks in here as aggregates are added.)</summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IApplicationDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICommand<TResponse>)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
