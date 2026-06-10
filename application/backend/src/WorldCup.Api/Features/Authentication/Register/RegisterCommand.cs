using WorldCup.Api.Common.Cqrs;

namespace WorldCup.Api.Features.Authentication.Register;

public sealed record RegisterCommand(string Email, string Password) : ICommand<RegisterResponse>;
