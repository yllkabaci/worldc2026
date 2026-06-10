namespace WorldCup.Domain.Exceptions;

/// <summary>Base type for business failures. Carries an <see cref="ErrorCodes"/> the API maps to an HTTP status.</summary>
public abstract class DomainException(ErrorCodes code, string message) : Exception(message)
{
    public ErrorCodes Code { get; } = code;
}
