
namespace Anar.Services.Notify;

using Microsoft.Extensions.Options;
using System.Text;

/// <summary>
/// Service to send notifications to NTFY service.
/// </summary>
/// <param name="httpClientFactory"></param>
/// <param name="queue"></param>
/// <param name="options"></param>
/// <param name="spamFilter"></param>
/// <param name="logger"></param>
internal sealed class NotifyService(
    IHttpClientFactory httpClientFactory,
    INotifyQueue queue,
    IOptions<NotifyOptions> options,
    ISpamFilter spamFilter,
    ILogger<NotifyService> logger
) : BackgroundService
{
    /// <summary>
    /// Send a single notification to the NTFY service.  Exposed for testing.
    /// </summary>
    /// <param name="alert">Alert to publish</param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    internal async Task SendNotificationAsync(Alert alert, CancellationToken stoppingToken)
    {
        // For now just using ToString() as the message body.
        var message = alert.ToString();
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        using var httpClient = httpClientFactory.CreateClient(nameof(NotifyService));
        var rs = await httpClient.PostAsync(string.Empty, content, stoppingToken);
        if (!rs.IsSuccessStatusCode)
        {
            logger.LogWarning(LogEvents.NotifySendError, "Failed to send notification: {StatusCode}", rs.StatusCode);
        }
    }

    /// <summary>
    /// Process all of the notifications in the queue. Exposed for testing.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    internal async Task ProcessNotificationsAsync(CancellationToken stoppingToken)
    {
        spamFilter.FlushExpired();
        while (!stoppingToken.IsCancellationRequested && queue.TryDequeue(out Alert? alert))
        {
            if (!spamFilter.IsOkToSend(alert!))
            {
                continue;
            }
            logger.LogInformation(LogEvents.NotifySending, "Sending {Alert}", alert);
            await SendNotificationAsync(alert!, stoppingToken);
        }
    }

    /// <summary>
    /// On each tick of the timer, process all of the notifications in the
    /// queue.
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(options.Value.PollingInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessNotificationsAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // These are expected on CTRL-C shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(LogEvents.NotifyExecuteError, ex, "Error in NotifyService");
        }
    }
}