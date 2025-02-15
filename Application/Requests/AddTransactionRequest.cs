namespace Application.Requests;

public class AddTransactionRequest
{
    public string TransactionType { get; set; }
    public decimal Amount { get; set; } 
    
}