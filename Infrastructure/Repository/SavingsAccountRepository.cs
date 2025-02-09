using Domain.Aggregates;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class SavingsAccountRepository : AggregateRepository<SavingsAccountAggregate>, ISavingsAccountRepository
{
    public SavingsAccountRepository(ILogger<AggregateRepository<SavingsAccountAggregate>> logger, IAggregateStore aggregateStore) : base(logger, aggregateStore)
    {
        
    }
}