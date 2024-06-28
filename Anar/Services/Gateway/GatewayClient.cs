using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Anar.Services;

internal sealed class GatewayClient : IGatewayClient
{
    private readonly ILogger<GatewayClient> _logger;
    private readonly GatewayOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Match up inverter serialNumber to a location.
    /// </summary>
    /// <param name="serialNumber"></param>
    /// <returns></returns>
    private Location? Locate(string serialNumber)
    {
        return Array.Find(_options.Layout, l => l.SerialNumber == serialNumber);
    }

    public GatewayClient(
        ILogger<GatewayClient> logger,
        IOptions<GatewayOptions> options,
        HttpClient httpClient
    ) {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient;

        var count = _options.Layout.Length;
        if (count == 0) {
            _logger.LogWarning("No array layout specified");
        } else {
            _logger.LogInformation("Using array layout with {Count} locations", count);
        }
    }

    /// <summary>
    /// Get how often to poll inverters.
    /// </summary>
    public TimeSpan Interval => _options.Interval;

    /// <summary>
    /// Get inverter data
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Inverter>> GetInvertersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Get inverters");
        var path = "api/v1/production/inverters";
        try {
            var inverters = await _httpClient
                .GetFromJsonAsync<Inverter[]>(path, cancellationToken) ?? [];

            // Do a bit of normalization.
            inverters = inverters.OrderBy(e => e.SerialNumber).ToArray();

            // Apply location data if available.
            Array.ForEach(inverters, i => {
                i.Location = Locate(i.SerialNumber);
            });

            return inverters;
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to get inverters");
            return [];
        }
    }
}