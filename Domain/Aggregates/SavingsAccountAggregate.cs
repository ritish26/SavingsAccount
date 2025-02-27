using Domain.Events;

namespace Domain.Aggregates;

public class SavingsAccountAggregate : AggregateRoot
{
    public string? BankName { get;  set; }
    public long BankId { get;  set; }
    public long AccountId { get;  set; }
    public decimal Balance { get;  set; }
    public override string Id { get; set; }

    // Private parameterless constructor (for event sourcing replay)
    public SavingsAccountAggregate() { } 
    
    public SavingsAccountAggregate(string? bankName, decimal balance, long bankId, long accountId)
    {
        if (string.IsNullOrEmpty(bankName))
        {
            throw new ArgumentNullException(nameof(bankName));
        }
        if (balance < 0)
        {
            throw new InvalidDataException(bankName + " can't be less than 0");
        }
        if (bankId <= 0)
        {
            throw new InvalidDataException(bankName + " can't be less than 0");
        }
        if (accountId <= 0)
        {
            throw new InvalidDataException("Account can't be less than 0");
        }

        RaiseEvent(new SavingsAccountCreated(bankName, bankId, accountId, balance, 1));
    }

    public void AddTransaction(string transactionType, decimal amount)
    {
        if (amount < 0)
        {
            throw new InvalidDataException(amount + " can't be less than 0");
        }
        
        RaiseEvent(new TransactionAdded($"{BankId}-{AccountId}", transactionType, amount, Version+1));
    }
    public void When(SavingsAccountCreated @event)
    {
        Id = $"{@event.BankId}-{@event.AccountId}";
        BankName = @event.BankName;
        BankId = @event.BankId;
        AccountId = @event.AccountId;
        Balance = @event.Balance;
        Version = @event.Version;
    }

    public void When(TransactionAdded @event)
    {
        if (@event.TransactionType.Equals("Credit", StringComparison.InvariantCultureIgnoreCase))
        {
            Balance += @event.Amount;
        }

        if (@event.TransactionType.Equals("Debit", StringComparison.InvariantCultureIgnoreCase))
        {
            Balance -= @event.Amount;
        }
        
        Version = @event.Version;
    }

}