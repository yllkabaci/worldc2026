using WorldCup.Domain.Exceptions;

namespace WorldCup.Domain.Users;

public sealed class EmailAlreadyExistsException(string email)
    : DomainException(ErrorCodes.EmailAlreadyExists, $"An account with email '{email}' already exists.");

public sealed class InvalidCredentialsException()
    : DomainException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
