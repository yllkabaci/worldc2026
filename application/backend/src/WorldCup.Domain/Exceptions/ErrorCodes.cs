namespace WorldCup.Domain.Exceptions;

/// <summary>Stable error-code taxonomy (WC-NNNN). Mapped to HTTP status by the global exception handler.</summary>
public enum ErrorCodes
{
    // 0000-0099 API errors
    ValidationError,   // WC-0001
    NotFound,          // WC-0002
    Conflict,          // WC-0003
    Unauthorized,      // WC-0004
    Forbidden,         // WC-0005

    // 1000-1999 External service errors
    FootballApiUnavailable, // WC-1001

    // 5000-5999 Feature / business-rule violations
    EmailAlreadyExists, // WC-5101
    InvalidCredentials, // WC-5102
}
