using System.Diagnostics;
using Domain.Events;
using Infrastructure.EventStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Projections;

namespace Infrastructure.Projections.Grains;

public interface IViewProjectionGrain : IGrainWithStringKey
{
    Task ProcessEvents(GrainCancellationToken grainCancellationToken);
}

public class ViewProjectionGrain : Grain, IViewProjectionGrain
{
    private readonly ILogger<ViewProjectionGrain> _logger;
    private readonly ITenantProjectionManagerFactory _tenantProjectionManagerFactory;
    private readonly IConfiguration _configuration;
    private readonly IEventStore _eventStore;
    private readonly IProjectionCheckpointStore _projectionCheckpointStore;
    
    private ITenantProjectionManager _tenantProjectionManager;
    private long _tenantId;

    public ViewProjectionGrain(ILogger<ViewProjectionGrain> logger,
        ITenantProjectionManagerFactory tenantProjectionManagerFactory,
        IConfiguration configuration,
        IEventStore eventStore,
        IProjectionCheckpointStore projectionCheckpointStore)
    {
        _logger = logger;
        _tenantProjectionManagerFactory = tenantProjectionManagerFactory;
        _configuration = configuration;
        _eventStore = eventStore;
        _projectionCheckpointStore = projectionCheckpointStore;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var primaryKey = this.GetPrimaryKeyString();

        if (!long.TryParse(primaryKey, out var tenantId))
        {
            throw new ArgumentException("IssuerId is invalid, Cannot Process events");
        }

        _tenantProjectionManager = _tenantProjectionManagerFactory.Create(tenantId);
        _tenantId = tenantId;
        
        await base.OnActivateAsync(cancellationToken);
    }
    public async Task ProcessEvents(GrainCancellationToken grainCancellationToken)
    {
        var streamName =
            $"{_configuration.GetSection("EventsStoreSettings:ChangeLogProjectionName").Value}.{_tenantId}";

        StreamEvent[] currentSlice;
        
        var eventsProcessed = 0;
        var batchSize = 100;
        var lastEventProcessed = long.MaxValue;
        var maxEventNumber = await _eventStore.GetLastEventNumber(streamName);
        
        //stopwatch to calculate the time taken to process the events 
        var stopwatch = Stopwatch.StartNew();
        do
        {
            var checkpointValue =
                (await _projectionCheckpointStore.GetCheckpoint(_tenantId))
                ?.CheckpointNumber ?? -1;

            var fromVersion = checkpointValue + 1;

            //If the checkpoint value is equal to the max event number, then there are no more events to processed
            if (checkpointValue == maxEventNumber)
            {
                break;
            }

            //Read events from tenant stream
            currentSlice = await _eventStore.ReadStream(streamName, fromVersion, batchSize);

            if (currentSlice.Length <= 0)
            {
                continue;
            }

            await _tenantProjectionManager.HandleEvents(
                currentSlice, grainCancellationToken.CancellationToken);

            lastEventProcessed = currentSlice.Max(streamEvent => streamEvent.EventNumber);

            await _projectionCheckpointStore.SaveCheckpoint(_tenantId, streamName, lastEventProcessed);

            eventsProcessed += currentSlice.Length;

        } while (lastEventProcessed < maxEventNumber && stopwatch.ElapsedMilliseconds < 360_000 &&
                 !grainCancellationToken.CancellationToken.IsCancellationRequested);
            
        stopwatch.Stop();
        
    }

}