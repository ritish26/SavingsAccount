namespace Domain.Aggregates;

public class ChangeLogProjectionCheckpointRecorded : BaseDomainEvent
{
    public ChangeLogProjectionCheckpointRecorded(long changelogEventNumber, DateTime lastEventProcessedTime) :
        base(Guid.NewGuid().ToString(), nameof(ChangeLogProjectionCheckpointRecorded), changelogEventNumber)
    {
        ChangelogEventNumber = changelogEventNumber;
        LastEventProcessedTime = lastEventProcessedTime;
    }
    
    public long ChangelogEventNumber { get;  set; }
    
    public DateTime LastEventProcessedTime { get;  set; }
}