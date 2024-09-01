using System.Collections.Concurrent;

namespace Anar.Services.Notify;

internal class NotifyQueue : ConcurrentQueue<Alert>, INotifyQueue
{ }