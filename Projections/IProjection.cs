using Domain.Events;

namespace Projections;

public interface IProjection
{
    Task HandleEvents(IEnumerable<StreamEvent> @event);
}