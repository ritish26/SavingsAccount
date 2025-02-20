using Domain.Aggregates;
using Domain.Events;
using EventStore.Client;
using Infrastructure.EventStore;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Projections;

public class ChangelogPartitionBackgroundService : BackgroundService
{
    private readonly ILogger<ChangelogPartitionBackgroundService> _logger;
    private readonly IEventStore _eventStore;
    private readonly IConfiguration _configuration;
    private readonly ITenantViewProjection _tenantViewProjection;

    private readonly string _metadataChangeLogEventNumber;

    public ChangelogPartitionBackgroundService(ILogger<ChangelogPartitionBackgroundService> logger,
        IEventStore eventStore,IConfiguration configuration,ITenantViewProjection tenantViewProjection)
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
            _eventStore.GetStreamObservable(changeLogProjectionName, Math.Max(checkPoint,0));
        
        
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