namespace Anar.Services.Notify;

using Anar.Services;
using Microsoft.Extensions.Options;

internal sealed class NotifyWorker(
    INotifyProcessor notifyProcessor,
    IOptions<NotifyOptions> options,
    ILogger<NotifyWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(options.Value.PollingInterval);
            logger.LogDebug("Starting notification polling {Period}", timer.Period);
            while (!stoppingToken.IsCancellationRequested)
            {
                await notifyProcessor.ProcessNotificationsAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // These are expected on CTRL-C shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(LogEvents.NotifyWorkerError, ex, "Error processing notifications");
        }
    }
}