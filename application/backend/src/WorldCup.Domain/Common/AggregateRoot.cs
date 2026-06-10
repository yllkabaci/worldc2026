namespace WorldCup.Domain.Common;

/// <summary>Base class for aggregate roots with a strongly-typed id and a domain-event buffer.</summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public abstract class AggregateRoot<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
