using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Options;

namespace Anar.Services.Gateway;

internal sealed class ClientHandler : HttpClientHandler
{
    private readonly IOptions<GatewayOptions> _options;

    public ClientHandler(IOptions<GatewayOptions> options)
    {
        _options = options;
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
        SslPolicyErrors errors
    ) => errors == SslPolicyErrors.None
      || _options.Value.Thumbprint == cert?.Thumbprint;
}