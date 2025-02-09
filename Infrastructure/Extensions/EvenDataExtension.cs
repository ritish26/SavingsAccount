

using Domain.Aggregates;
using Domain.Events;
using Infrastructure.EventStore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Extensions;

public static class EvenDataExtension
{
    public static EventData ToEventData(this object @event,  Dictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return new EventData(@event, metadata);
    }

    public static BaseDomainEvent? ToDomainEvent(string eventType, string eventData)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(eventData);

        Type? type = GetTypeByEventType(eventType);

        if (type == null)
        {
            
        }

        return JsonConvert.DeserializeObject(eventData,type) as BaseDomainEvent;

    }

    private static Type? GetTypeByEventType(string eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetReferencedAssemblies().Any(a => a.Name == "System.Web" || a.Name == "System.Data.Linq"))
            {
                continue;
            }
            
            Type? type = assembly.GetExportedTypes().FirstOrDefault(t => t.FullName == eventType);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
}