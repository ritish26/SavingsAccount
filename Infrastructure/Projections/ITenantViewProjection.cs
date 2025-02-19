namespace Infrastructure;

public interface ITenantViewProjection
{
    Task ProcessEvents(string tenantId);
}