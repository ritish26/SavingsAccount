using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EventStore.Client;

namespace Infrastructure.Extensions;

public static class EventStoreExtensions
{
    public static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("EventStoreSettings:ConnectionString").Value;
        if (connectionString != null)
        {
            var settings = EventStoreClientSettings.Create(connectionString);
            var client = new EventStoreClient(settings);
            services.AddSingleton(client);
        }
        return services;
    }
}