using Domain.Aggregates;
using EventStore.Client;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ES = Infrastructure.EventStore;
namespace Infrastructure.Repository;

public class AggregateStore : IAggregateStore
{
    private readonly ILogger<AggregateStore> _logger;
    private readonly ES.IEventStore _eventStore;
    private readonly IConfiguration _configuration;

    public AggregateStore(IConfiguration configuration, ILogger<AggregateStore> logger,
        ES.IEventStore eventStoreClient)
    {
        _configuration = configuration;
        _logger = logger;
        _eventStore = eventStoreClient;
    }

    public async Task<TAggregate?> Load<TAggregate>(string aggregateId, bool useSnapshot) 
        where TAggregate : AggregateRoot
    {
        var streamName =
            $"{_configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value}-{aggregateId}";
        
        if(!await _eventStore.Exists(streamName))
        {
            return null;
        }
        
        long startVersion = 0;
        TAggregate? aggregate = null;
        if (useSnapshot)
        {
            aggregate = await LoadSnapshot<TAggregate>(aggregateId);
            if (aggregate != null)
            {
                startVersion = aggregate.Version + 1;
            }
        }

        try
        {
            var events = await _eventStore.ReadStream(streamName,
                startVersion, int.MaxValue);


            aggregate ??= (TAggregate)Activator.CreateInstance(typeof(TAggregate), false);
            aggregate.ReplayEvents(events.Select(streamEvents => streamEvents.Event).ToList());
        }

        catch (StreamNotFoundException)
        {
            return null;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
        }
        
        return aggregate;
    }

    private async Task<TAggregate?> LoadSnapshot<TAggregate>(string aggregateId) where TAggregate : AggregateRoot
    {
        TAggregate? aggregate = null;
        
        var streamName =
            $"{_configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value}-{aggregateId}";
        
        try
        {
            var events = await _eventStore.ReadStream(streamName, -1, 1, Direction.Backwards);
            var snapShotStreamCreated = events.FirstOrDefault()?.Event;

            if (snapShotStreamCreated is SnapshotCreated snapshotCreated)
            {
                aggregate = snapshotCreated.Aggregate as TAggregate;
            }
        }

        catch (StreamNotFoundException)
        {
            aggregate = null;
        }
        
        return aggregate;
    }

    public async Task Save<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot
    {
        if (aggregate == null)
        {
            throw new ArgumentNullException(nameof(aggregate));
        }
        
        var changes = aggregate.GetUncommittedChanges().ToList();

        if (changes.Count == 0)
        {
            _logger.LogInformation("No changes to save");
            return;
        }
        
        long aggregateInitialVersion = aggregate.Version - changes.Count;

        var eventsToWrite = changes.Select(ToeventData).ToList();

        var streamName = $"{_configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value}-{aggregate.Id}";

        if (aggregateInitialVersion == 0)
        {
            // New Stream
           if(await _eventStore.Exists(streamName))
            {
                throw new ES.DuplicateStreamException(streamName);
            }
            
            await _eventStore.CreateNewStream(streamName, eventsToWrite);
        }

        else
        {
            await _eventStore.AppendEventsToStream(streamName, eventsToWrite);
        }
        
        aggregate.MarkChangesAsCommitted();
        await SaveSnapshot(aggregate);
    }

    private async Task SaveSnapshot<TAggregate>(TAggregate aggregate, bool force=false) where TAggregate : AggregateRoot
    {
        long? lastSnapshotVersion = null;
        if (!force)
        {
            var lastSnapshot = await LoadSnapshot<TAggregate>(aggregate.Id);
            if (lastSnapshot != null)
            {
                lastSnapshotVersion = lastSnapshot.Version;
            }
        }

        if (force || aggregate.Version - (lastSnapshotVersion ?? 0) >= 3)
        {
            var snapShotEventData = ToeventData(new SnapshotCreated(aggregate, aggregate.Version));
            var streamName = $"{_configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value}.snapshot-{aggregate.Id}";
            if (!await _eventStore.Exists(streamName))
            {
                await _eventStore.CreateNewStream(streamName, new[] { snapShotEventData });
            }

            else
            {
                await _eventStore.AppendEventsToStream(streamName, new[] { snapShotEventData });
            }
            
        }
    }
    private ES.EventData ToeventData(BaseDomainEvent @event)
    {
        var dict = new Dictionary<string, string>();
        var eventData = @event.ToEventData(dict);
        return eventData;
    }
}


public class SnapshotCreated : BaseDomainEvent
{
    public object Aggregate { get; }

    public SnapshotCreated(object aggregate, long version) : base(Guid.NewGuid().ToString(), 
        nameof(SnapshotCreated),version)
    {
        Aggregate = aggregate ?? throw new ArgumentNullException(nameof(aggregate));
    }
}