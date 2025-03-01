using Domain.Aggregates;
using Domain.Views;

namespace Projections.ViewBuilders;

public class ViewBuilder<T> where T : IViewDocument
{
    private T _data;

    public void SetView(T data)
    {
        this._data = data ?? throw new ArgumentNullException(nameof(data));
        InitData();
    }

    public void Apply(BaseDomainEvent @event)
    {
        var method = GetType().GetMethod("When", new[] { @event.GetType() });
        if (method == null)
        {
            throw new ArgumentException("The given event does not implement IWhen");
        }
        
        method.Invoke(this, new[] { @event });
    }

    public T Data
    {
        get
        {
            if (_data == null)
            {
                throw new Exception("The view builder has not been initialized");
            }
            return _data;
        }
    }

    /// <summary>
    /// Called after data is set. Can be used to initialize defaults on the data
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual void InitData()
    {
        throw new NotImplementedException();
    }
}