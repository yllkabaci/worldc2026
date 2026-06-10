namespace WorldCup.Domain.Abstractions;

/// <summary>Abstraction over the current time so time-dependent rules are deterministically testable.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
