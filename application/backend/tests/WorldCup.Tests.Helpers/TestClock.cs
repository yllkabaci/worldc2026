using WorldCup.Domain.Abstractions;

namespace WorldCup.Tests.Helpers;

/// <summary>Deterministic clock for tests; set <see cref="UtcNow"/> to control time-dependent rules (deadlines, etc.).</summary>
public sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}
