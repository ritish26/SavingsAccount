namespace Application.Commands.AddTransaction;

public class AddTransactionCommand : ICommand
{
    public string AccountId { get; set; }
    public long BankId { get; set; }
    public string? TransactionType {get; set;}
    public decimal Amount { get; set; }
}