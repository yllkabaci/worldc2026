using Microsoft.EntityFrameworkCore;
using WorldCup.Api.Common.Cqrs;
using WorldCup.Domain.Abstractions;
using WorldCup.Domain.Users;

namespace WorldCup.Api.Features.Authentication.Register;

public sealed class RegisterHandler(IApplicationDbContext db, IPasswordHasher hasher, IClock clock)
    : ICommandHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new EmailAlreadyExistsException(email);
        }

        var user = User.Register(email, hasher.Hash(command.Password), clock.UtcNow);
        db.Users.Add(user);
        // UnitOfWork behavior commits.

        return new RegisterResponse { UserId = user.Id.Value, Email = user.Email };
    }
}
