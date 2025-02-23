using Domain.Events;

namespace Projections;

public class TenantProjectionManager : ITenantProjectionManager
{
    private readonly IProjection[] _projections;

    public TenantProjectionManager(IProjection[] projections)
    {
        _projections = projections;
    }
    public async Task HandleEvents(IReadOnlyList<StreamEvent> events, CancellationToken grainCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);

        var streamEventsArray = events.ToList();

        foreach (var streamEvent in streamEventsArray)
        {
            await Task.WhenAll(_projections.Select(async projection =>
                await projection.HandleEvents([streamEvent])));
        }
    }
}