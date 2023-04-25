namespace PresalesApp.Database
{
    // https://stackoverflow.com/a/531450
    internal class QueriesQueue<T>
    {
        private readonly Queue<T> _queue = new();

        public virtual bool Any() => _queue.Any();

        public event EventHandler? OnEnqueued;

        public event EventHandler? OnReachedEmpty;

        protected virtual void OnEnqueue() => OnEnqueued?.Invoke(this, EventArgs.Empty);

        protected virtual void OnReachEmpty() => OnReachedEmpty?.Invoke(this, EventArgs.Empty);

        public virtual void Enqueue(T item)
        {
            _queue.Enqueue(item);
            OnEnqueue();
        }

        public virtual T Dequeue()
        {
            T item = _queue.Dequeue();
            if (!this.Any()) OnReachEmpty();
            return item;
        }

    }
}
