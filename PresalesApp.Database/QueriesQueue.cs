namespace PresalesApp.Database;

// https://stackoverflow.com/a/531450
internal class QueriesQueue<T>
{
    private readonly Queue<T> _Queue = new();

    public virtual bool Any() => _Queue.Count != 0;

    public event EventHandler? OnEnqueued;

    public event EventHandler? OnReachedEmpty;

    protected virtual void OnEnqueue() => OnEnqueued?.Invoke(this, EventArgs.Empty);

    protected virtual void OnReachEmpty() => OnReachedEmpty?.Invoke(this, EventArgs.Empty);

    public virtual void Enqueue(T item)
    {
        _Queue.Enqueue(item);
        OnEnqueue();
    }

    public virtual T Dequeue()
    {
        var item = _Queue.Dequeue();
        if (!Any()) OnReachEmpty();
        return item;
    }
}
