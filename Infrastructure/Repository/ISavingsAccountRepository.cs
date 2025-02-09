using Domain.Aggregates;

namespace Infrastructure.Repository;

public interface ISavingsAccountRepository : IAggregateRepository<SavingsAccountAggregate>
{
    
}