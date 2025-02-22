using Infrastructure.Projections.Grains;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Infrastructure.Projections;

public class TenantViewProjection : ITenantViewProjection
{
    private readonly ILogger<TenantViewProjection> _logger;
    private readonly IGrainFactory _grainFactory;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    
    public TenantViewProjection(ILogger<TenantViewProjection> logger,
        IGrainFactory grainFactory, IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _grainFactory = grainFactory;
        _hostApplicationLifetime = applicationLifetime;
    }
    public async Task ProcessEvents(string tenantId)
    {
        var grainCancellationTokenSource = new GrainCancellationTokenSource();
        _hostApplicationLifetime.ApplicationStopping.Register(() =>
        {
            grainCancellationTokenSource.Cancel();
        });
        
        var projectionGrain = _grainFactory.GetGrain<IViewProjectionGrain>(tenantId);
        
        await projectionGrain.ProcessEvents(grainCancellationTokenSource.Token);
    }
}