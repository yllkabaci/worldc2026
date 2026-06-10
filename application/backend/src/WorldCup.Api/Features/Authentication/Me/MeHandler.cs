using WorldCup.Api.Common.Cqrs;
using WorldCup.Domain.Abstractions;

namespace WorldCup.Api.Features.Authentication.Me;

public sealed class MeHandler(ICurrentUserService currentUser) : IQueryHandler<GetMeQuery, MeResponse>
{
    public Task<MeResponse> Handle(GetMeQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(new MeResponse
        {
            UserId = currentUser.UserId ?? Guid.Empty,
            Email = currentUser.Email ?? string.Empty,
            Roles = currentUser.Roles,
        });
}
