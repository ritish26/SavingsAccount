using EventStore.Client;
using Grpc.Core;

namespace Infrastructure.EventStore;

public class EventStoreGrpcStreamObservable : IObservable<ResolvedEvent>
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly string _streamName;
    private long? _lastCheckpoint;

    public EventStoreGrpcStreamObservable(EventStoreClient eventStoreClient,
        string streamName, long? lastCheckpoint)
    {
        _eventStoreClient = eventStoreClient;
        _streamName = streamName;
        _lastCheckpoint = lastCheckpoint;
    }
    
    public IDisposable Subscribe(IObserver<ResolvedEvent> observer)
    {
        var subscription = new EventStoreGrpcSubscription(_eventStoreClient,
            observer, _streamName, _lastCheckpoint);
        
        return subscription;
    }
}

public class EventStoreGrpcSubscription : IDisposable
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly IObserver<ResolvedEvent> _observer;
    private readonly string _streamName;
    private const int MaxReSubscriptionRetryCount = 3;
    private static readonly TimeSpan MaxReSubscriptionRetryInterval = TimeSpan.FromSeconds(5);
    private StreamSubscription _subscription;
    private long? _lastCheckpoint;

    public EventStoreGrpcSubscription(EventStoreClient eventStoreClient, IObserver<ResolvedEvent> observer,
        string streamName, long? lastCheckpoint)
    {
        _eventStoreClient = eventStoreClient;
        _streamName = streamName;
        _lastCheckpoint = lastCheckpoint;
        _observer = observer;
    }

    private async Task InitAsync()
    {
        _subscription?.Dispose();
        
        var fromStream = _lastCheckpoint is null or <=0
            ? FromStream.Start
            : FromStream.After(StreamPosition.FromInt64(_lastCheckpoint!.Value));

        _subscription = await _eventStoreClient.SubscribeToStreamAsync(_streamName, fromStream,
            OnEventArrived, true, OnsubscriptionDropped);
    }

    private void OnsubscriptionDropped(StreamSubscription subscription, SubscriptionDroppedReason reason, Exception? exception)
    {
        if (reason != SubscriptionDroppedReason.Disposed &&
            exception is not RpcException
            {
               StatusCode : StatusCode.Cancelled
            })
        {
            Resubscribe();
            return;
        }
        _observer.OnCompleted();
    }

    private void Resubscribe()
    {
        var retryCount = 0;

        while (retryCount < MaxReSubscriptionRetryCount)
        {
            var resubscribed = false;
            try
            {
                InitAsync().Wait();
                resubscribed = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (resubscribed)
            {
                break;
            }
            
            Thread.Sleep(MaxReSubscriptionRetryInterval);
            retryCount++;
        }

        if (retryCount < MaxReSubscriptionRetryCount)
        {
            return;
        }
        
        _observer.OnError(new Exception("Resubscribe failed " + _streamName));
    }

    private Task OnEventArrived(StreamSubscription subscription,
        ResolvedEvent resolvedEvent, CancellationToken ct = default)
    {
        try
        {
            _observer.OnNext(resolvedEvent);
        }
        catch (Exception e)
        {
            _observer.OnError(e);
        }
        finally
        {
            _lastCheckpoint = resolvedEvent.OriginalEventNumber.ToInt64();
        }
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
    }
}