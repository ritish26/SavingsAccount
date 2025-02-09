using System.Data;
using Domain.Events;

namespace Domain.Aggregates;

public abstract class AggregateRoot
{
    private readonly List<BaseDomainEvent> _changes = new();
    public long Version { get; set; }
    public abstract string Id { get; set; }
    public IEnumerable<BaseDomainEvent> GetUncommittedChanges()
    {
        return _changes;
    }
    public void MarkChangesAsCommitted()
    {
        _changes.Clear();
    }
    
    private void Apply(BaseDomainEvent @event, bool isReplay = false)
    {
        var method = GetType().GetMethod("When", new[] { @event.GetType() });
        
        if (method == null)
        {
            throw new ArgumentNullException($"When event type {@event.GetType()} not found");
        }

        method.Invoke(this, new[] { @event });
        
        if (isReplay)
        {
            return;
        }

        if (Version <= @event.Version && !_changes.Any(x => Equals(x.Version, @event.Version)))
        {
            _changes.Add(@event);
            Version= @event.Version;
        }

        else
        {
            if (nameof(@event).Equals(nameof(SavingsAccountCreated)))
            {
                var @eventTemp = @event as SavingsAccountCreated;
                throw new DuplicateNameException($"This Account is already a saving account {@eventTemp.AccountId}");
            }
        }
        
    }

    protected void RaiseEvent(BaseDomainEvent domainEvent)
    {
        Apply(@domainEvent);
    }

    public void ReplayEvents(List<BaseDomainEvent?> domainEvents)
    {
        foreach (var @event in domainEvents)
        {
            if (@event != null) Apply(@event);
        }
    }
}