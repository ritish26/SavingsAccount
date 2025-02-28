using System.Diagnostics;
using Domain.Events;
using Infrastructure.EventStore;
using Microsoft.Extensions.Configuration;
using Projections;

namespace Infrastructure.Projections.Grains;

public interface IViewProjectionGrain 
{
    Task ProcessEvents(CancellationTokenSource grainCancellationToken, string tenantId);
}

public class ViewProjectionGrain : IViewProjectionGrain
{
    private readonly IConfiguration _configuration;
    private readonly IProjectionCheckpointStore _projectionCheckpointStore;
    private readonly IEventStore _eventStore;
    
    private ITenantProjectionManager _tenantProjectionManager;
    private long _tenantId;

    public ViewProjectionGrain(
        IConfiguration configuration,
        IEventStore eventStore,
        IProjectionCheckpointStore projectionCheckpointStore
        )
    {
        _configuration = configuration;
        _eventStore = eventStore;
        _projectionCheckpointStore = projectionCheckpointStore;
    }
    
    public async Task ProcessEvents(CancellationTokenSource grainCancellationToken, string tenantId)
    {
        _tenantId = long.Parse(tenantId);
        var streamName =
            $"{_configuration.GetSection("EventStoreSettings:ChangeLogProjectionName").Value}.{_tenantId}";

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
                currentSlice, grainCancellationToken);

            lastEventProcessed = currentSlice.Max(streamEvent => streamEvent.EventNumber);

            await _projectionCheckpointStore.SaveCheckpoint(_tenantId, streamName, lastEventProcessed);

            eventsProcessed += currentSlice.Length;

        } while (lastEventProcessed < maxEventNumber && stopwatch.ElapsedMilliseconds < 360_000 &&
                 !grainCancellationToken.IsCancellationRequested);
            
        stopwatch.Stop();
        
    }

}