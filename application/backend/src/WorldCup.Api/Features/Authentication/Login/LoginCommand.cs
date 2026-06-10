using WorldCup.Api.Common.Cqrs;

namespace WorldCup.Api.Features.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
