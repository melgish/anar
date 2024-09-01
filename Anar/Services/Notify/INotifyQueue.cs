namespace Anar.Services.Notify;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// a FIFO queue for in-application alerts
/// </summary>
internal interface INotifyQueue
{
    /// <summary>
    /// Add an alert to the queue
    /// </summary>
    /// <param name="message"></param>
    public void Enqueue(Alert message);

    /// <summary>
    /// Try to pull an alert from the queue
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool TryDequeue([MaybeNullWhen(false)] out Alert? message);
}