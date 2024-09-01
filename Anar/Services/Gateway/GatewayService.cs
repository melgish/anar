// spell-checker: words Enphase
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using Anar.Services.Notify;


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
    ILogger<GatewayService> logger,
    INotifyQueue notifyQueue
) : IGatewayService
{
    // If the token is invalid, the status code will be 401 and the message will read:
    // "Response status code does not indicate success: 401 (Unauthorized)."
    private bool Is401UnauthorizedError(Exception ex)
        => ex is HttpRequestException rex && rex.StatusCode == HttpStatusCode.Unauthorized;
    // IF the SSL certificate does not validate the thumbprint, the status code
    // will be null, and the message will read:
    // "The remote certificate was rejected by the provided RemoteCertificateValidationCallback."
    private bool IsSSLThumbprintError(Exception ex)
         => ex.GetBaseException() is AuthenticationException auth && auth.Message.Contains("RemoteCertificateValidationCallback");

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
            if (Is401UnauthorizedError(ex))
            {
                logger.LogWarning(LogEvents.GetInvertersFailed, "Authorization Failure");
                notifyQueue.Enqueue(new AuthenticationAlert("Unauthorized"));
            }
            else if (IsSSLThumbprintError(ex))
            {
                // Thumbprint validator will already have enqueued the alert
                // with the additional information not available here.
                logger.LogWarning(LogEvents.GetInvertersFailed, "SSL Certificate Error");
            }
            else
            {
                logger.LogWarning(LogEvents.GetInvertersFailed, ex, "Failed to get inverters");
            }
            return [];
        }
    }
}