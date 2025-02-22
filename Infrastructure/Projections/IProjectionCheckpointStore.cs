namespace Infrastructure.Projections;

public record EventStreamProjectionCheckpoint(long CheckpointNumber, DateTime? LastCheckpoint);

/// <summary>
/// This is used to store the checkpoint for mongo projections
/// </summary>
public interface IProjectionCheckpointStore
{
   Task<EventStreamProjectionCheckpoint?> GetCheckpoint(long streamName);

   Task SaveCheckpoint(long tenantId, string projectionName, long checkpoint);

}