namespace Application.Requests;

public class CreateSavingsAccountRequest
{
    public string? BankName { get; set; }
    
    public long BankId { get; set; }
    
    public long AccountId { get; set; }
    public decimal Balance { get; set; }
    
}