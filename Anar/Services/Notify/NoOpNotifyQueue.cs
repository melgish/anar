namespace Anar.Services.Notify;

/// <summary>
/// A no-op implementation of <see cref="INotifyQueue"/> that is always empty.
/// </summary>
internal sealed class NoOpNotifyQueue : INotifyQueue
{
    public void Enqueue(Alert message) { }

    public bool TryDequeue(out Alert? message)
    {
        message = null;
        return false;
    }
}