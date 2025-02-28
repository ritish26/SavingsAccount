using System.Diagnostics;
using System.Reactive.Linq;
using Domain.Aggregates;
using Domain.Events;
using EventStore.Client;
using Infrastructure.EventStore;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EventData = Infrastructure.EventStore.EventData;

namespace Infrastructure.Projections;

public class ChangelogPartitionBackgroundService : BackgroundService
{
    private readonly ILogger<ChangelogPartitionBackgroundService> _logger;
    private readonly IEventStore _eventStore;
    private readonly IConfiguration _configuration;
    private readonly ITenantViewProjection _tenantViewProjection;

    private readonly string _metadataChangeLogEventNumber;

    public ChangelogPartitionBackgroundService(ILogger<ChangelogPartitionBackgroundService> logger,
        IEventStore eventStore, IConfiguration configuration, ITenantViewProjection tenantViewProjection)
    {
        _logger = logger;
        _eventStore = eventStore;
        _configuration = configuration;
        _tenantViewProjection = tenantViewProjection;

        _metadataChangeLogEventNumber = $"Bank.Savings_Account.Changelog.EventNumber";

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        var checkPoint = await GetChangeLogCheckPoint();

        var changeLogProjectionName =
            $"{_configuration.GetSection("EventStoreSettings:ChangeLogProjectionName").Value}";
        var streamObservable =
            _eventStore.GetStreamObservable(changeLogProjectionName, Math.Max(checkPoint, 0));

        var lastEventProcessed = 0;
        var eventsSinceLastCheckpoint = 0;
        var lastCheckpointTime = DateTime.UtcNow;

        streamObservable.Buffer(TimeSpan.FromMilliseconds(500), 5).Subscribe(resolvedEvents =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Stream {StreamName} is cancelled due to Cancellation Token",
                        changeLogProjectionName);
                    return;
                }

                var stopWatch = Stopwatch.StartNew();

                if (resolvedEvents.Count > 0)
                {
                    HandleEvents(resolvedEvents).Wait(stoppingToken);

                    lastEventProcessed = (int)resolvedEvents.Max(@event =>
                        @event.OriginalEventNumber.ToInt64());

                    eventsSinceLastCheckpoint += resolvedEvents.Count;
                }

                stopWatch.Stop();
                if (DateTime.UtcNow - lastCheckpointTime > TimeSpan.FromMilliseconds(500) &&
                    eventsSinceLastCheckpoint > 0)
                {
                    SaveCheckpoint(lastEventProcessed).Wait(stoppingToken);

                    eventsSinceLastCheckpoint = 0;
                    lastCheckpointTime = DateTime.UtcNow;
                }
            }, ex => { _logger.LogError(ex, "Error during stream processing"); },
            () => { _logger.LogInformation("Stream {StreamName} is processed", changeLogProjectionName); },
            stoppingToken);
    }

    private async Task SaveCheckpoint(long checkPoint)
    {
        var changeLogProjectionName = $"{_configuration.GetSection(
            "EventStoreSettings:ChangeLogProjectionName").Value}";

        if (string.IsNullOrWhiteSpace(changeLogProjectionName))
        {
            throw new ApplicationException($"No changelog projection configured {changeLogProjectionName}");
        }

        if (checkPoint < 0)
        {
            throw new ApplicationException($"No checkpoint configured {checkPoint}");
        }
        
        var streamName = $"{changeLogProjectionName}.{checkPoint}";
        var lastEvent = await GetLastEvent(streamName);

        var changeLogCheckpoint = lastEvent.Event as ChangeLogProjectionCheckpointRecorded;
        if (changeLogCheckpoint?.ChangelogEventNumber > checkPoint)
        {
            return;
        }

        var checkpointEvent =
            new ChangeLogProjectionCheckpointRecorded(checkPoint, DateTime.UtcNow);

        await _eventStore.AppendEventsToStream(streamName, new[]
        {
            checkpointEvent.ToEventData(new Dictionary<string,string>())
        });
    }

    private async Task HandleEvents(IList<ResolvedEvent> resolvedEvents)
    {
        var tenantEventsMap = new Dictionary<string, List<ResolvedEvent>>();

        foreach (var @event in resolvedEvents)
        {
            var tenantId = @event.Event.EventStreamId.Split(".")[2].Split("-")[0];

            if (string.IsNullOrEmpty(tenantId))
            {
                continue;
            }
            
            tenantEventsMap.Add(tenantId, new List<ResolvedEvent>());
            tenantEventsMap[tenantId].Add(@event);
        }

        foreach (var tenantEvent in tenantEventsMap)
        {
            await LinkEventsToTenantStream(tenantEvent.Key, tenantEvent.Value);
        }

        if (tenantEventsMap.Count > 0)
        {
            var tenantIds = tenantEventsMap.Keys.ToArray();
            await ProcessTenantEvents(tenantIds);
        }
    }

    private async Task ProcessTenantEvents(string[] tenantIds)
    {
        foreach (var tenantId in tenantIds)
        {
            await _tenantViewProjection.ProcessEvents(tenantId);
        }
    }

    private async Task LinkEventsToTenantStream(string tenantId, IReadOnlyList<ResolvedEvent> resolvedEvents)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        if (resolvedEvents != null && !resolvedEvents.Any())
        {
            return;
        }
        
        var streamName = $"{_configuration.GetSection(
            "EventStoreSettings:ChangeLogProjectionName").Value}.{tenantId}";

        var linkEvents = new List<EventData>();

        var lastLinkedEventsDetails = await GetLastLinkedEventDetails(streamName);
        long changeLogEventNumber = -1;
        
        foreach(var @event in resolvedEvents)
        {
            changeLogEventNumber = @event.OriginalEventNumber.ToInt64();

            // If the event is already linked to the tenant stream then return
            if (lastLinkedEventsDetails.HasValue &&
                lastLinkedEventsDetails.Value.ChangeLogEventNumber >= changeLogEventNumber)
            {
                continue;
            }

            var metadata = new Dictionary<string, string>()
            {
                { _metadataChangeLogEventNumber, $"{changeLogEventNumber}" }
            };

            var eventData = $"{@event.Event.EventNumber}@{@event.Event.EventStreamId}";
            
            linkEvents.Add(eventData.ToEventData(metadata));
        }
        
        await _eventStore.AppendEventsToStream(streamName, linkEvents, true);
        
    }

    private async Task<(long ChangeLogEventNumber, long lastEventNumber)?>
    GetLastLinkedEventDetails(string streamName)
    {
        ArgumentNullException.ThrowIfNull(streamName);

        var streamExists = await _eventStore.Exists(streamName);
        if (!streamExists)
        {
            return null;
        }
        
        var lastStreamEventSlice = await _eventStore.ReadStream(streamName,
            -1, 1, Direction.Backwards);

        var lastEvent = lastStreamEventSlice.FirstOrDefault();
        if (lastEvent == null)
        {
            throw new InvalidOperationException(
                $"Stream {streamName} does not exist");
        }

        var eventMetaData = lastEvent.Metadata;

        if (!eventMetaData.TryGetValue(_metadataChangeLogEventNumber, out var lastChangeEventNumber))
        {
            throw new InvalidOperationException(
                $"MetaData {_metadataChangeLogEventNumber} is not found in {streamName}");
        }

        if (!long.TryParse(lastChangeEventNumber, out var changeLogEventNumber))
        {
            throw new InvalidOperationException(
                $"Unable to parse {_metadataChangeLogEventNumber} in {streamName}");
        }
        
        return (changeLogEventNumber, lastStreamEventSlice.First().EventNumber);
    }
    

    private async Task<long> GetChangeLogCheckPoint()
    {
        var changeLogProjectionName =
            $"{_configuration.GetSection("EventStoreSettings:ChangeLogProjectionName").Value}";

        if (string.IsNullOrWhiteSpace(changeLogProjectionName))
        {
            throw new InvalidOperationException("No change log projection configured.");
        }

        var streamName = $"{changeLogProjectionName}.Checkpoint";
        var lastEvent = await GetLastEvent(streamName);

        if (lastEvent is null)
        {
            throw new InvalidOperationException("Last event in stream {streamName} was not found.");
        }

        return lastEvent.EventNumber;
    }
    
    private async Task<StreamEvent> GetLastEvent(string streamName)
    {
        if (streamName is null)
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (!await _eventStore.Exists(streamName))
        {
            var checkpointEvent = new ChangeLogProjectionCheckpointRecorded(-1, default);

            await _eventStore.CreateNewStream(streamName, new[]
            {
                checkpointEvent.ToEventData(new Dictionary<string, string>())
            });
        }

        var slice = await _eventStore.ReadStream(streamName, -1,
            1, Direction.Backwards);

        if (slice is null || !slice.Any())
        {
            throw new InvalidOperationException(
                $"Unable to retrieve last event {streamName}.");
        }

        return slice.First();
    }
}