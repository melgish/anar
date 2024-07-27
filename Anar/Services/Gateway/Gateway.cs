// spell-checker: words Enphase
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

namespace Anar.Services.Gateway;

internal interface IGateway
{
    /// <summary>
    /// Requests inverter data from the Enphase gateway.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// List of inverter info when successful, otherwise an empty list</returns>
    Task<IList<Inverter>> GetInvertersAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Minimal client for the Enphase gateway API.
/// </summary>
/// <param name="httpClient"></param>
/// <param name="logger"></param>
internal sealed class Gateway : IGateway
{
    const string Path = "api/v1/production/inverters";
    private readonly HttpClient _httpClient;

    private readonly IOptions<GatewayOptions> _options;
    private readonly ILogger<Gateway> _logger;

    public Gateway(HttpClient httpClient, IOptions<GatewayOptions> options, ILogger<Gateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        httpClient.BaseAddress = options.Value.Uri;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.Value.Token);
    }

    /// <summary>
    /// Get inverter data
    /// </summary>
    /// <returns>List of inverter data or empty list</returns>
    public async Task<IList<Inverter>> GetInvertersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Get inverters");
        try
        {
            var data = await _httpClient.GetFromJsonAsync<Inverter[]>(
                _options.Value.RequestPath,
                cancellationToken
            );
            return data?.OrderBy(e => e.SerialNumber).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get inverters");
            return [];
        }
    }
}