using Domain.Aggregates;

namespace Infrastructure.Repository;

public interface IAggregateStore
{
    Task<TAggregate?> Load<TAggregate>(string aggregateId, bool useSnapshot) where TAggregate : AggregateRoot;
    
    Task Save<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot;
}