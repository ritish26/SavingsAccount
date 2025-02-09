namespace Domain.Aggregates;

public abstract class BaseDomainEvent
{
    public BaseDomainEvent(string id, string type, long version)
    {
        Id = id;
        Type = type;
        Version = version;
        OccuredOn = DateTime.UtcNow;
    }
    public string Id { get; set; }

    public string Type { get; set; }

    public long Version { get; set; }

    public DateTime OccuredOn { get; set; } 
}