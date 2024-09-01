using System.Collections.Concurrent;

namespace Anar.Services.Notify;

/// <summary>
/// Simple implementation of the FIFO queue.
/// </summary>
internal sealed class NotifyQueue : ConcurrentQueue<Alert>, INotifyQueue
{ }