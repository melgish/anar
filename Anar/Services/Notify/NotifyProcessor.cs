
namespace Anar.Services.Notify;

using System.Text;

interface INotifyProcessor
{
    Task ProcessNotificationsAsync(CancellationToken stoppingToken);
}


/// <summary>
/// Service to send notifications to NTFY service.
/// </summary>
/// <param name="httpClientFactory"></param>
/// <param name="queue"></param>
/// <param name="spamFilter"></param>
/// <param name="logger"></param>
internal sealed class NotifyProcessor(
    IHttpClientFactory httpClientFactory,
    INotifyQueue queue,
    ISpamFilter spamFilter,
    ILogger<NotifyProcessor> logger
) : INotifyProcessor
{
    /// <summary>
    /// Send a single notification to the NTFY service.  Internal for testing.
    /// </summary>
    /// <param name="alert">Alert to publish</param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    internal async Task SendNotificationAsync(Alert alert, CancellationToken stoppingToken)
    {
        logger.LogInformation(LogEvents.NotifySending, "Sending {Alert}", alert);
        // For now just using ToString() as the message body.
        var message = alert.ToString();
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        using var httpClient = httpClientFactory.CreateClient(nameof(NotifyProcessor));
        var rs = await httpClient.PostAsync(string.Empty, content, stoppingToken);
        if (!rs.IsSuccessStatusCode)
        {
            logger.LogWarning(LogEvents.NotifySendError, "Failed to send notification: {StatusCode}", rs.StatusCode);
        }
    }

    /// <summary>
    /// Process all of the notifications in the queue..
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task ProcessNotificationsAsync(CancellationToken stoppingToken)
    {
        spamFilter.FlushExpired();
        while (
            !stoppingToken.IsCancellationRequested &&
            queue.TryDequeue(out Alert? alert) &&
            spamFilter.IsOkToSend(alert!)
        )
        {
            await SendNotificationAsync(alert!, stoppingToken);
        }
    }
}