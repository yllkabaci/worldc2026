using WorldCup.Api.Common.Cqrs;

namespace WorldCup.Api.Features.Authentication.Me;

public sealed record GetMeQuery : IQuery<MeResponse>;
