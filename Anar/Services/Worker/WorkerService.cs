using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;

using Microsoft.Extensions.Options;

namespace Anar.Services.Worker;

internal sealed class WorkerService(
    WorkerOptions options,
    ILocatorService locator,
    IGatewayService gatewayService,
    IInfluxService influxService,
    ILogger<WorkerService> logger
) : BackgroundService
{
    public WorkerService(
        IOptions<WorkerOptions> options,
        ILocatorService locator,
        IGatewayService gatewayService,
        IInfluxService influxService,
        ILogger<WorkerService> logger
    ) : this(options.Value, locator, gatewayService, influxService, logger) { }

    /// <summary>
    /// Reads data from IQ Gateway and pushes it to InfluxDB
    /// </summary>
    /// <param name="stoppingToken"></param>
    internal async Task ProcessData(CancellationToken stoppingToken)
    {
        logger.LogDebug("Processing");
        var inverters = await gatewayService.GetInvertersAsync(stoppingToken);
        if (inverters.Count == 0)
        {
            // Nothing to write.
            return;
        }

        // Combine inverter data with location data.
        var readings = (
            from inverter in inverters
            join location in locator.Locations on
            inverter.SerialNumber equals location.SerialNumber into locations
            from location in locations.DefaultIfEmpty()
            select new Reading(inverter, location)
        ).ToList();

        // Calculate the total output from all inverters and use latest
        // timestamp from the readings as 'now'
        var wattsNow = Totals.FromReadings(readings);
        logger.LogInformation(LogEvents.CurrentOutput, "CurrentOutput {WattsNow}", wattsNow);

        await influxService.WritePointsAsync(
            readings
                .Select(r => r.ToPointData())
                .Append(wattsNow.ToPointData())
                .ToList()
        , stoppingToken);
    }

    /// <summary>
    /// Periodically reads data from IQ Gateway and pushes it to InfluxDB
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting polling");
            using var timer = new PeriodicTimer(options.Interval);
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessData(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // These are expected on CTRL-C shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Worker error");
        }
    }
}