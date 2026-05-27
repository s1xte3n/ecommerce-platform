using ECommerce.Domain.Base;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Persistence;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchEventsAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken = default)
    {
        // Simplified - in production, dispatch to MediatR or message bus
        return Task.CompletedTask;
    }
}
