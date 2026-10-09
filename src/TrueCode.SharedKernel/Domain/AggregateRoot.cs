namespace TrueCode.SharedKernel.Domain;

public abstract class AggregateRoot<Tid> : Entity<Tid>, IEventSourceable
    where Tid : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(Tid id) : base(id) { }

    public void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}