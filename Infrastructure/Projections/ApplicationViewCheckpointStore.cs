using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Projections;

namespace Infrastructure.Projections;

public class ApplicationViewCheckpointStore : IProjectionCheckpointStore
{
    private readonly ILogger<ApplicationViewCheckpointStore> _logger;
    private readonly MongoContext _mongoContext;

    public ApplicationViewCheckpointStore(
        ILogger<ApplicationViewCheckpointStore> logger, MongoContext mongoContext)
    {
        _logger = logger;
        _mongoContext = mongoContext;
    }
    public async Task<EventStreamProjectionCheckpoint?> GetCheckpoint(long tenantId)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }

        var checkpointViewCollection = _mongoContext.GetCollection<TenantProjectionCheckpoint>();
        var doc = await checkpointViewCollection.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (doc == null)
        {
            return null;
        }

        return new EventStreamProjectionCheckpoint(doc.Checkpoint, doc.LastCheckpoint);
    }

    public async Task SaveCheckpoint(long tenantId, string projectionName, long checkpoint)
    {
        if (string.IsNullOrEmpty(projectionName))
        {
            throw new ArgumentNullException(nameof(projectionName));
        }

        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        
        var checkpointViewCollection = _mongoContext.GetCollection<TenantProjectionCheckpoint>();
        
        var updateBuilder = Builders<TenantProjectionCheckpoint>.Update;
        var filterBuilder = Builders<TenantProjectionCheckpoint>.Filter;

        var result = await checkpointViewCollection.UpdateOneAsync(filterBuilder.Eq(x => x.TenantId, tenantId),
            updateBuilder.Combine(updateBuilder.Set(xx => xx.Checkpoint, checkpoint),
                updateBuilder.Set(x => x.LastCheckpoint, DateTime.UtcNow)));

        if (result.MatchedCount == 0)
        {
            await InsertDocument(projectionName, tenantId, checkpoint, DateTime.UtcNow);
        }
    }

    private async Task InsertDocument(string name,
        long tenantId, long checkpoint, DateTime? lastCheckPointDateTime)
    {
        var checkpointViewCollection = _mongoContext.GetCollection<TenantProjectionCheckpoint>();
        await checkpointViewCollection.InsertOneAsync(new TenantProjectionCheckpoint 
            {
                TenantId = tenantId, 
                Checkpoint = checkpoint,
                Name = name,
                LastCheckpoint = lastCheckPointDateTime,
                CreatedAt = DateTime.Now
            });
    }
}