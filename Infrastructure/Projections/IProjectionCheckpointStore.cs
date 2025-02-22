namespace Infrastructure.Projections;

public record EventStreamProjectionCheckpoint(long CheckpointNumber, DateTime? LastCheckpoint);
public interface IProjectionCheckpointStore
{
   Task<EventStreamProjectionCheckpoint?> GetCheckpoint(long streamName);

   Task SaveCheckpoint(long tenantId, string projectionName, long checkpoint);

}