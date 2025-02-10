using System.Runtime.Serialization;
using Domain.Aggregates;

namespace Domain.Events;

public class SavingsAccountCreated : BaseDomainEvent
{
    public SavingsAccountCreated(string? bankName, long bankId, long accountId, decimal balance, long version) :
        base($"{bankId}-{accountId}",nameof(SavingsAccountCreated), version)
    {
       BankName = bankName;
       BankId = bankId;
       AccountId = accountId;
       Balance = balance;
    }
    public string? BankName { get;  set; }
    
    public long BankId { get;  set; }
    
    public long AccountId { get;  set; }
    
    public decimal Balance { get;  set; }
}

public class TransactionAdded : BaseDomainEvent
{
    public TransactionAdded(string id, string transactionType, decimal amount, long version) : 
        base(id, nameof(TransactionAdded), version)
    {
        TransactionType = transactionType;
        Amount = amount;
    }
    
    public string TransactionType { get;  set; }
    
    public decimal Amount { get;  set; }
}