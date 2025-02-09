using Domain.Aggregates;
using Domain.Events;
using Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public interface IAggregateRepository<TAggregate> where TAggregate : AggregateRoot
{
    Task Create(Func<Task<TAggregate>> createFunc);
    
}

public abstract class AggregateRepository<TAggregate> : IAggregateRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    protected IAggregateStore AggregateStore { get; }
    private readonly ILogger<AggregateRepository<TAggregate>> _logger;
    
    protected AggregateRepository(ILogger<AggregateRepository<TAggregate>> logger, IAggregateStore aggregateStore)
    {
        _logger = logger;
        AggregateStore = aggregateStore;
    }
    
    public async Task Create(Func<Task<TAggregate>> createFunc)
    {
        ArgumentNullException.ThrowIfNull(createFunc);
        var aggregate = await createFunc();
        await AggregateStore.Save(aggregate);
    }
}