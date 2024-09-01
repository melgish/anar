
using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Options;

namespace Anar.Services.Notify;

internal sealed class NotifyService(
    IHttpClientFactory httpClientFactory,
    INotifyQueue queue,
    IOptions<NotifyOptions> options,
    ISpamFilter spamFilter,
    ILogger<NotifyService> logger
) : BackgroundService
{
    private async Task SendNotificationAsync(Alert alert, CancellationToken stoppingToken)
    {
        var message = alert switch
        {
            SimpleAlert simple => simple.ToString(),
            ThumbprintAlert thumbprint => thumbprint.ToString(),
            AuthenticationAlert auth => auth.ToString(),
            _ => alert.ToString()
        };
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        using var httpClient = httpClientFactory.CreateClient(nameof(NotifyService));
        var rs = await httpClient.PostAsync(string.Empty, content);
        if (!rs.IsSuccessStatusCode)
        {
            logger.LogWarning(LogEvents.NotifySendError, "Failed to send notification: {StatusCode}", rs.StatusCode);
        }
    }

    /// <summary>
    /// Process all of the notifications in the queue.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    private async Task ProcessNotificationsAsync(CancellationToken stoppingToken)
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