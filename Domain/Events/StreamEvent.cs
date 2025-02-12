using Domain.Aggregates;

namespace Domain.Events;

public  class StreamEvent
{
    public StreamEvent(BaseDomainEvent? @event, long eventNumber, Dictionary<string, string> metadata)
    {
        Event = @event;
        EventNumber = eventNumber;
        Metadata = metadata;
    }
    
    public BaseDomainEvent? Event { get; set; } 
    
    public long EventNumber { get; set; }
    
    public Dictionary<string, string> Metadata { get; set; }
}