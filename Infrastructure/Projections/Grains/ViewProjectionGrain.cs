using Orleans;

namespace Infrastructure.Projections.Grains;

public interface IViewProjectionGrain : IGrainWithStringKey
{
    Task ProcessEvents(CancellationToken grainCancellationToken);
}

public class ViewProjectionGrain : Grain, IViewProjectionGrain
{
    public Task ProcessEvents(CancellationToken grainCancellationToken)
    {
        throw new NotImplementedException();
    }
}