namespace Projections;

public interface ITenantProjectionManagerFactory
{ 
    public ITenantProjectionManager Create(long tenantId);
    
}