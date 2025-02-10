using Domain.Events;

namespace Domain.Aggregates;

public class SavingsAccountAggregate : AggregateRoot
{
    public string? BankName { get; private set; }
    public long BankId { get; private set; }
    public long AccountId { get; private set; }
    public decimal Balance { get; private set; }
    public override string Id { get; set; }

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

        RaiseEvent(new SavingsAccountCreated(bankName, bankId, accountId, balance, 0));
    }

    public void AddTransaction(string transactionType, decimal amount)
    {
        if (amount < 0)
        {
            throw new InvalidDataException(amount + " can't be less than 0");
        }
        
        RaiseEvent(new TransactionAdded($"{BankId}--{AccountId}", transactionType, amount, Version+1));
    }
    public void When(SavingsAccountCreated @event)
    {
        Id = $"{@event.BankName}-{@event.AccountId}";
        BankName = @event.BankName;
        BankId = @event.BankId;
        AccountId = @event.AccountId;
        Balance = @event.Balance;
        Version = @event.Version;
    }

    public class TransactionAdded : BaseDomainEvent
    {
        public TransactionAdded(string id, string transactionType, decimal amount, long version) :
            base(id, nameof(TransactionAdded), version)
        {
            TransactionType = transactionType;
            Amount = amount;
        }

        public string TransactionType { get; set; }

        public decimal Amount { get; set; }
    }
}