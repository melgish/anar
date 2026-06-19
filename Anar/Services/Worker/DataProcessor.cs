namespace Anar.Services.Worker;

using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;

interface IDataProcessor
{
    Task ProcessDataAsync(CancellationToken stoppingToken);
}

internal sealed class DataProcessor(
    ILocatorService locator,
    IGatewayService gatewayService,
    IInfluxService influxService,
    ILogger<DataProcessor> logger
) : IDataProcessor
{
    /// <summary>
    /// Reads data from IQ Gateway and pushes it to InfluxDB
    /// </summary>
    /// <param name="stoppingToken"></param>
    public async Task ProcessDataAsync(CancellationToken stoppingToken)
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
}