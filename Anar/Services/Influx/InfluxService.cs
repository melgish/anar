using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

using Microsoft.Extensions.Options;

namespace Anar.Services;

internal sealed class InfluxService : IInfluxService {
    private readonly ILogger<InfluxService> _logger;
    private readonly InfluxOptions _options;
    public InfluxService(
        ILogger<InfluxService> logger, 
        IOptions<InfluxOptions> options
    ) {
        _logger = logger;
        _options = options.Value;
    }

    public async Task WriteAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default) {
        // Do not log empty collections.
        if (!inverters.Any()) {
            return;
        }
        try {
            using var client = new InfluxDBClient(_options.Uri.ToString(), _options.Token);
            var api = client.GetWriteApiAsync();

            // Original timestamp on the inverter to eliminate duplicates.
            // Total will be logged using most recent report.
            var points = inverters.Select(i => i.ToPointData()).ToArray();

            await api.WritePointsAsync(
                points, 
                _options.Bucket, 
                _options.Organization, 
                cancellationToken
            );

            // Summing watts for current output.
            // Use most-recent inverter report to set time on total.
            var wattsNow = inverters.Sum(i => i.LastReportWatts);
            var now = inverters.Max(i => i.LastReportDate);
            _logger.LogInformation(LogEvent.CurrentOutput, "CurrentOutput {WattsNow}", wattsNow);

            await api.WritePointAsync(PointData
                .Measurement("totals")
                .Field("wattsNow", wattsNow)
                .Timestamp(now, WritePrecision.S),
                _options.Bucket,
                _options.Organization,
                cancellationToken
            );
        }
        catch (Exception ex) {
            _logger.LogWarning(LogEvent.WriteInvertersFailed, ex, "Failed to get inverters");
        }
    }
}