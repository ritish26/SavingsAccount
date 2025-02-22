namespace Projections;

public class TenantProjectionCheckpoint
{
    public string Name { get; set; }
    
    public long TenantId { get; set; }
    
    public long Checkpoint { get; set; }
    
    public DateTime? LastCheckpoint { get; set; }
    
    public DateTime CreatedAt { get; set; }
}