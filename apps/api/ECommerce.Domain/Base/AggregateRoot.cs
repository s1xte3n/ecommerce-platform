// File: apps/api/src/Domain/Base/AggregateRoot.cs
namespace ECommerce.Domain.Base;

/// <summary>
/// Base class for aggregate roots.
/// Manages domain events for eventual consistency patterns.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public interface IDomainEvent { }
