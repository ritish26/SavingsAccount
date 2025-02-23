using Domain.Events;
using Domain.Views;
using Microsoft.Extensions.Logging;

namespace Projections.ViewBuilders;

public class SavingsAccountViewBuilder : ViewBuilder<SavingsAccountView>
{
    private readonly ILogger<SavingsAccountViewBuilder> _logger;

    public SavingsAccountViewBuilder(ILogger<SavingsAccountViewBuilder> logger)
    {
        _logger = logger;
    }

    public void When(SavingsAccountCreated @event)
    {
        Data.Id = $"{@event.BankId}-{@event.AccountId}";
        Data.BankName = @event.BankName;
        Data.BankId = @event.BankId;
        Data.AccountId = @event.AccountId;
        Data.Balance = @event.Balance;
    }

    public void When(TransactionAdded @event)
    {
        if (@event.TransactionType.Equals("Credit", StringComparison.InvariantCultureIgnoreCase))
        {
            Data.Balance += @event.Amount;
        }
        else if (@event.TransactionType.Equals("Debit", StringComparison.InvariantCultureIgnoreCase))
        {
           if(Data.Balance - @event.Amount < 0)
           {
               throw new InvalidOperationException("Debit not enough");
           }
        }
        Data.Balance += @event.Amount;
    }
    
}