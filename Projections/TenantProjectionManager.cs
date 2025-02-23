using Domain.Events;

namespace Projections;

public class TenantProjectionManager : ITenantProjectionManager
{
    /// <summary>
    /// Manages multiple projections by distributing stream events to them asynchronously.
    /// </summary>
    private readonly IProjection[] _projections;

    public TenantProjectionManager(IProjection[] projections)
    {
        _projections = projections;
    }
    
    /// <summary>
    /// Handles events by distributing them to all registered projections asynchronously.
    /// </summary>
    /// <param name="events">List of stream events to process.</param>
    /// <param name="grainCancellationToken">Cancellation token for async operations.</param>
    public async Task HandleEvents(IReadOnlyList<StreamEvent> events, CancellationToken grainCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);

        var streamEventsArray = events.ToList();

        foreach (var streamEvent in streamEventsArray)
        {
            // Process all events in parallel across projections
            await Task.WhenAll(_projections.Select(async projection =>
                await projection.HandleEvents([streamEvent])));
        }
    }
}