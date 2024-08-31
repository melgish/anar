// spell-checker: words Enphase
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

namespace Anar.Services.Gateway;

internal interface IGatewayService
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
internal sealed class GatewayService(
    IHttpClientFactory httpClientFactory,
    ILogger<GatewayService> logger
) : IGatewayService
{
    /// <summary>
    /// Get inverter data
    /// </summary>
    /// <returns>List of inverter data or empty list</returns>
    public async Task<IList<Inverter>> GetInvertersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug(nameof(GetInvertersAsync));
        using var httpClient = httpClientFactory.CreateClient(nameof(GatewayService));
        try
        {

            var data = await httpClient.GetFromJsonAsync<Inverter[]>(string.Empty, cancellationToken);
            return data?.OrderBy(e => e.SerialNumber).ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(LogEvents.GetInvertersFailed, ex, "Failed to get inverters");
            return [];
        }
    }
}