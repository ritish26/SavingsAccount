namespace Infrastructure.EventStore;

public class EventData
{
    public EventData(object payload, Dictionary<string, string> metadata)
    {
        EventId = Guid.NewGuid();
        Payload = payload;
        Metadata = metadata;
    }

    public Guid EventId { get; }
    
    public object Payload { get; set; }
    
    public Dictionary<string, string> Metadata { get; set; }
}