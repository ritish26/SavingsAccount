using Domain.Aggregates;
using Domain.Events;
using Domain.Views;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Projections.ViewBuilders;

namespace Projections;

public abstract class MongoProjection<TProjectionView, TProjectionViewBuilder> : IProjection
    where TProjectionView : IViewDocument where TProjectionViewBuilder : ViewBuilder<TProjectionView>
{
    private readonly IServiceProvider _serviceProvider;
    protected IMongoCollection<TProjectionView> Collection { get; }

    public MongoProjection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var mongoContext = _serviceProvider.GetRequiredService<IMongoContext>();
        Collection = mongoContext.GetCollection<TProjectionView>();
    }
    public async Task HandleEvents(IEnumerable<StreamEvent> @event)
    {
        foreach (var streamEvent in @event)
        {
            var @domainEvent = @streamEvent.Event;

            var views = await FindViews(@domainEvent);

            foreach (var viewDocument in views)
            {
                if (viewDocument is null)
                {
                    throw new InvalidOperationException($"No view found for type {typeof(TProjectionView).Name}");
                }

                var prevTimeStamp = viewDocument.LastEventTimestamp;

                if (viewDocument.LastEventTimestamp >= streamEvent.Event.OccuredOn.Ticks)
                {
                    continue;
                }

                var viewBuilder = CreateViewBuiler(viewDocument);

                if (viewBuilder.Data == null)
                    throw new InvalidOperationException($"No view built for type {typeof(TProjectionView).Name}");
                
                viewBuilder.Apply(streamEvent.Event);
                viewBuilder.Data.LastEventTimestamp = streamEvent.Event.OccuredOn.Ticks;

                await SaveView(viewBuilder.Data, prevTimeStamp);
            }
        }
      
    }
    private ViewBuilder<TProjectionView> CreateViewBuiler(
        TProjectionView viewDocument)
    {
        var viewBuilder = _serviceProvider.GetRequiredService<TProjectionViewBuilder>();
        viewBuilder.SetView(viewDocument);
        return viewBuilder;
    }

    protected abstract Task<IEnumerable<TProjectionView>> FindViews(BaseDomainEvent domainEvent);
    
    protected abstract Task SaveView(TProjectionView viewBuilder, long prevTimeStamp);
} 