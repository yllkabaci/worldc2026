using WorldCup.Domain.Abstractions;

namespace WorldCup.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
