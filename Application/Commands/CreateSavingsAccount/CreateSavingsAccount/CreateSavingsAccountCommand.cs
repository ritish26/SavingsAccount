namespace Application.Commands;

public class CreateSavingsAccountCommand : ICommand
{
    public string? BankName { get; set; }
    
    public long BankId { get; set; }
    
    public long AccountId { get; set; }
    
    public decimal Balance { get; set; }
}