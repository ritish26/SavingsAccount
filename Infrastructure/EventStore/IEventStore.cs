using Domain.Events;
using EventStore.Client;

namespace Infrastructure.EventStore;

public interface IEventStore
{
    Task<bool> Exists(string streamName);

    Task<StreamEvent[]> ReadStream(string streamName, long fromVersion, int count,
        Direction direction = Direction.Forwards);
    
    Task  CreateNewStream(string streamName, IEnumerable<EventData> events, bool isLinkType = false);
    
    Task AppendEventsToStream(string streamName, IEnumerable<EventData> events, bool isLinkType = false);
    
    Task<long> GetLastEventNumber(string streamName);
    
}