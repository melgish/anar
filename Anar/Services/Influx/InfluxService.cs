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

    public async Task WriteTotalsAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default) {
        // Do not log empty collections.
        if (!inverters.Any())
        {
            _logger.LogDebug("No totals to write");
            return;
        }

        // https://enphase.com/download/accessing-iq-gateway-local-apis-or-local-ui-token-based-authentication
        // Early on, it was possible to read totals from an endpoint.
        // /api/v1/production. At some point my system firmware was upgraded and
        // that endpoint only returns zeros.  The alternative endpoint
        // ivp/pdm/energy does not work either. That one returns a 401.

        // So instead just add up all the inverter values.

        // Summing watts for current output.
        // Use most-recent inverter report to set time on total.
        var wattsNow = inverters.Sum(i => i.LastReportWatts);
        var now = inverters.Max(i => i.LastReportDate);
        _logger.LogInformation("CurrentOutput {Timestamp} {WattsNow}", now, wattsNow);

        try {
            using var client = new InfluxDBClient(_options.Uri.ToString(), _options.Token);
            var api = client.GetWriteApiAsync();
            await api.WritePointAsync(PointData
                .Measurement("totals")
                .Field("wattsNow", wattsNow)
                .Timestamp(now, WritePrecision.S),
                _options.Bucket,
                _options.Organization,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write totals data");
        }
    }

    public async Task WriteInvertersAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default) {
        // Do not log empty collections.
        if (!inverters.Any()) {
            _logger.LogDebug("No inverters to write");
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
            _logger.LogInformation("CurrentOutput {Timestamp} {WattsNow}", now, wattsNow);

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
            _logger.LogWarning(ex, "Failed to write inverter data");
        }
    }
}