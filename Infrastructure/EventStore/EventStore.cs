using System.Data;
using System.Text;
using Domain.Events;
using EventStore.Client;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ES = EventStore.Client;

namespace Infrastructure.EventStore;

public class EventStore : IEventStore
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly ILogger<EventStore> _logger;

    public EventStore(EventStoreClient eventStoreClient, ILogger<EventStore> logger)
    {
        _eventStoreClient = eventStoreClient;
        _logger = logger;
    }

    public async Task<bool> Exists(string streamName)
    {
        ArgumentNullException.ThrowIfNull(streamName);

        var lastEventNumber = await GetLastEventNumber(streamName);

        var streamExist = lastEventNumber != -1;

        return streamExist;
    }

    public async Task<long> GetLastEventNumber(string streamName)
    {
        ArgumentNullException.ThrowIfNull(streamName);

        try
        {
            var lastEvent = await ReadStream(streamName, -1, 1, Direction.Backwards);
            return lastEvent.First().EventNumber;
        }
        catch (StreamNotFoundException)
        {
            return -1;
        }
    }

    public IObservable<ResolvedEvent> GetStreamObservable(string streamName, long fromVersion)
    {
        ArgumentNullException.ThrowIfNull(streamName);
        
        long? checkPoint = null;

        if (fromVersion > 0)
        {
            checkPoint = fromVersion - 1;
        }
        
        return new EventStoreGrpcStreamObservable(_eventStoreClient, streamName, checkPoint);
    }

    public async Task<StreamEvent[]> ReadStream(string streamName, long fromVersion, int count,
        Direction direction = Direction.Forwards)
    {
        ArgumentNullException.ThrowIfNull(streamName);

        if (count <= 0)
        {
            throw new ArgumentException($"'{nameof(count)}' cannot be less or equal to zero.", nameof(count));
        }

        var fromPosition = fromVersion switch
        {
            0 => StreamPosition.Start,
            1 => StreamPosition.End,
            _ => StreamPosition.FromInt64(fromVersion)
        };

        var resolvedLinkTos = true;
        var eventsToRead = count;

        var readStreamResult =
            _eventStoreClient.ReadStreamAsync(direction, streamName, fromPosition, eventsToRead,
                resolvedLinkTos);

        var readState = await readStreamResult.ReadState;
        
        if (readState == ReadState.StreamNotFound)
        {
            throw new StreamNotFoundException(streamName);
        }

        var streamEvents = new List<StreamEvent>();

        await foreach (var @event in readStreamResult)
        {
            var currentEventNumber = @event.OriginalEventNumber.ToInt64();
            
            var metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                Encoding.UTF8.GetString(@event.Event.Metadata.ToArray())) ?? new Dictionary<string, string>();
            if (@event.Link != null)
            {
                var linkMetadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    Encoding.UTF8.GetString(@event.Link.Metadata.ToArray())) ?? new Dictionary<string, string>();
                
                foreach (var eachLinkMetaData in linkMetadata)
                {
                    metadata.Add(eachLinkMetaData.Key, eachLinkMetaData.Value);
                }
            }

            var eventData = EventDataExtension.ToDomainEvent(@event.Event.EventType,
                Encoding.UTF8.GetString(@event.Event.Data.ToArray()));

            if (eventData == null)
            {
                continue;
            }

            streamEvents.Add(new StreamEvent(eventData, currentEventNumber, metadata));
        }

        return streamEvents.ToArray();
    }

    public async Task CreateNewStream(string streamName, IEnumerable<EventData> events, bool isLinkType = false)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(streamName);

        var eventArray = events.Select(
            @event => ToEventStoreEventData(@event, isLinkType)).ToArray();
        try
        {
            await _eventStoreClient.AppendToStreamAsync(streamName, StreamState.NoStream, eventArray);
        }
        catch (WrongExpectedVersionException ex) when (ex.ActualVersion >= 0)
        {
            _logger.LogInformation(ex, "Stream {streamName} already exists", streamName);
            throw new DuplicateStreamException(streamName);
        }
    }

    public async Task AppendEventsToStream(string streamName, IEnumerable<EventData> events, bool isLinkType = false)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(streamName);

        try
        {
            var eventArray = events.Select(@event => ToEventStoreEventData(@event, isLinkType)).ToArray();
            await _eventStoreClient.AppendToStreamAsync(streamName, StreamState.Any, eventArray);
        }
        catch (DBConcurrencyException ex)
        {
            throw new DBConcurrencyException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stream {Messgae} {stackTrace} failed to append to stream", ex.Message, ex.StackTrace);
        }
    }

    private ES.EventData ToEventStoreEventData(EventData @event, bool isLinkType)
    {
        var eventPayload = Encoding.UTF8.GetBytes(isLinkType
            ? @event.Payload as string ?? string.Empty
            : JsonConvert.SerializeObject(@event.Payload));
        
        var eventMetaData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event.Metadata));

        var eventStoreEventData = new ES.EventData(Uuid.NewUuid(),
            (isLinkType ? SystemEventTypes.LinkTo : @event.Payload.GetType().FullName ?? string.Empty), eventPayload,
            eventMetaData);

        return eventStoreEventData;
    }
}