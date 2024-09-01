namespace Anar.Services.Notify;

using System.Diagnostics.CodeAnalysis;

internal interface INotifyQueue
{
    public void Enqueue(Alert message);

    public bool TryDequeue([MaybeNullWhen(false)] out Alert? message);
}
