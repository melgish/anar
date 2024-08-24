using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Options;

namespace Anar.Services.Gateway;

internal sealed class ClientHandler : HttpClientHandler
{
    private readonly IOptions<GatewayOptions> _options;

    private readonly ILogger _logger;

    public ClientHandler(IOptions<GatewayOptions> options, ILogger<ClientHandler> logger)
    {
        _options = options;
        _logger = logger;
        ServerCertificateCustomValidationCallback = ValidateThumbprint;
    }

    /// <summary>
    /// Checks first for valid certificate, and failing that a certificate
    /// that matches the configured thumbprint.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cert"></param>
    /// <param name="chain"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    private bool ValidateThumbprint(
        HttpRequestMessage message,
        X509Certificate2? cert,
        X509Chain? chain,
        SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        if (_options.Value.Thumbprint.Equals(cert?.Thumbprint, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Log a less cryptic error than SSL could not be established.
        // Include actual in case it's the annual renew.
        _logger.LogError(
            "Invalid Thumbprint: Expected {Expected}, Actual {Actual}",
            _options.Value.Thumbprint,
            cert?.Thumbprint
        );

        return false;
    }

}