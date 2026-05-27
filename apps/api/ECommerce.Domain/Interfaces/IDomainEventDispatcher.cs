using ECommerce.Domain.Base;

namespace ECommerce.Domain.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken = default);
}
