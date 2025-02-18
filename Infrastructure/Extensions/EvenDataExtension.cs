

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Domain.Aggregates;
using Infrastructure.EventStore;
using Newtonsoft.Json;

namespace Infrastructure.Extensions;

public static class EventDataExtension
{
    public static EventData ToEventData<T>([DisallowNull] this T @event,  Dictionary<string, string> metadata)
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
            throw new ArgumentException($"Event type {eventType} not found or could not be resolved.");
        }

        if (type.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"Cannot deserialize generic type definition: {type.FullName}");
        }

        Console.WriteLine($"Deserializing event of type: {type.FullName}");

        return JsonConvert.DeserializeObject(eventData, type) as BaseDomainEvent;
    }

    private static Type? GetTypeByEventType(string eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetExportedTypes().FirstOrDefault(t => t.FullName == eventType);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
}