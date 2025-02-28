using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Infrastructure.Projections.Grains;

namespace Infrastructure.Projections;

public class TenantViewProjection : ITenantViewProjection
{
    private readonly ILogger<TenantViewProjection> _logger;
    private readonly IViewProjectionGrain _viewProjectionService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public TenantViewProjection(ILogger<TenantViewProjection> logger,
        IViewProjectionGrain viewProjectionService, 
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _viewProjectionService = viewProjectionService;
        _hostApplicationLifetime = applicationLifetime;
    }

    public async Task ProcessEvents(string tenantId)
    {
        using var cts = new CancellationTokenSource();
        
        _hostApplicationLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogInformation("Application is stopping. Cancelling event processing.");
            cts.Cancel();
        });

        try
        {
            await _viewProjectionService.ProcessEvents(cts,tenantId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event processing was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing events for tenant {TenantId}", tenantId);
        }
    }
}