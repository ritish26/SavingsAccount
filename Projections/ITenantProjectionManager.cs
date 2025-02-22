using Domain.Events;

namespace Projections;

public interface ITenantProjectionManager
{
    Task HandleEvents(IReadOnlyList<StreamEvent> events, CancellationToken grainCancellationToken);
}