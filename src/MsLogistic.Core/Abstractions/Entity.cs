namespace MsLogistic.Core.Abstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }
    private readonly List<DomainEvent> _domainEvents;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        Id = id;
        CreatedAt = DateTime.UtcNow;
        _domainEvents = [];
    }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
        _domainEvents = [];
    }

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}