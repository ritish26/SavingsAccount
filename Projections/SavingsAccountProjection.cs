using System.Reflection.Metadata;
using Domain.Aggregates;
using Domain.Events;
using Domain.Views;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Projections.ViewBuilders;

namespace Projections;

public class SavingsAccountProjection : MongoProjection<SavingsAccountView,SavingsAccountViewBuilder>
{
    private readonly ILogger<SavingsAccountProjection> _logger;
    public SavingsAccountProjection(ILogger<SavingsAccountProjection> logger, IServiceProvider serviceProvider) : 
        base(serviceProvider)
    {
        _logger = logger;
    }

    protected override async Task<IEnumerable<SavingsAccountView>> FindViews(BaseDomainEvent @event)
    {
        var view = await Collection.AsQueryable().FirstOrDefaultAsync(
            x => x.Id == @event.Id);
        
        if (@event is SavingsAccountCreated)
        {
            view ??= new SavingsAccountView();
        }

        if (view is null)
        {
            throw new InvalidOperationException(nameof(view));
        }
        
        return new[] { view };
    }

    protected override async Task SaveView(SavingsAccountView document, long prevTimeStamp)
    {
        await Collection.ReplaceOneAsync(
            x => x.Id == document.Id
                 &&
                 x.LastEventTimestamp == prevTimeStamp,document,
            new ReplaceOptions { IsUpsert = true });
    }
}