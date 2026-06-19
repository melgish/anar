namespace Anar.Services.Worker;

using Microsoft.Extensions.Options;

internal sealed class DataWorker(
    IDataProcessor dataProcessor,
    IOptions<DataOptions> options,
    ILogger<DataWorker> logger
) : BackgroundService
{
    /// <summary>
    /// Periodically reads data from IQ Gateway and pushes it to InfluxDB
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(options.Value.PollingInterval);
            logger.LogDebug("Starting data polling {Period}", timer.Period);
            while (!stoppingToken.IsCancellationRequested)
            {
                await dataProcessor.ProcessDataAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // These are expected on CTRL-C shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(LogEvents.DataWorkerError, ex, "Worker error");
        }
    }
}