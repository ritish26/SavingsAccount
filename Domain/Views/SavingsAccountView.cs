namespace Domain.Views;

public class SavingsAccountView : IViewDocument
{
    public string Id { get; set; }
    
    public string BankName { get; set; }
    
    public long BankId { get; set; }
    
    public long AccountId { get; set; }
    
    public decimal Balance { get; set; }
    
    public long LastEventTimestamp { get; set; }
}