namespace Application.Commands.AddTransaction;

public class AddTransactionCommand : ICommand
{
    public long AccountId { get; set; }
    
    public string? BankName { get; set; }
    
    public string? TransactionType {get; set;}
    
    public decimal Amount { get; set; }
}