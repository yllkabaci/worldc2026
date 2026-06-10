using Microsoft.EntityFrameworkCore;
using WorldCup.Api.Common.Cqrs;
using WorldCup.Domain.Abstractions;
using WorldCup.Domain.Users;

namespace WorldCup.Api.Features.Authentication.Login;

public sealed class LoginHandler(IApplicationDbContext db, IPasswordHasher hasher, IJwtIssuer jwt)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Treat unknown / blocked / wrong-password identically — never disclose which.
        if (user is null || !user.IsActive || !hasher.Verify(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = jwt.IssueToken(user.Id.Value, user.Email, new[] { user.Role.ToString() });
        return new LoginResponse { Token = token };
    }
}
