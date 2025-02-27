namespace Infrastructure.EventStore;

public interface IEventStoreAdmin
{
    Task CreateContinuousProjection(string projectionName, string query, bool enabled, bool emit,
        bool trackEmittedStreams);
    
    Task<bool> ProjectionExists(string projectionName);
}