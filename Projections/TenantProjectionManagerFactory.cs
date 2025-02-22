using Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Projections;

public class TenantProjectionManagerFactory : ITenantProjectionManagerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TenantProjectionManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public ITenantProjectionManager Create(long tenantId)
    {
        var projections = _serviceProvider.GetServices<IProjection>().ToArray();
        
        var tenantProjectionManager = new TenantProjectionManager(projections);
        
        return tenantProjectionManager;
    }
}