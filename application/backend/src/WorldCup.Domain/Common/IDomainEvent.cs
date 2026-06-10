namespace WorldCup.Domain.Common;

/// <summary>Marker for domain events raised by aggregates. Kept framework-free; the API adapts these to MediatR notifications when dispatching.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
