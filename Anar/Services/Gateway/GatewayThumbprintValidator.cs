namespace Anar.Services.Gateway;

using Anar.Services.Notify;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Validates the thumbprint of the certificate against the configured value.
/// </summary>
/// <param name="options"></param>
/// <param name="logger"></param>
/// <param name="notifyQueue"></param>
internal sealed class GatewayThumbprintValidator(
    IOptions<GatewayOptions> options,
    ILogger<GatewayThumbprintValidator> logger,
    INotifyQueue notifyQueue
)
{
    /// <summary>
    /// Validates that the thumbprint of the certificate matches the
    /// configured value.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cert"></param>
    /// <param name="chain"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool ValidateThumbprint(
        HttpRequestMessage message,
        X509Certificate2? cert,
        X509Chain? chain,
        SslPolicyErrors errors
    )
    {
        if (errors == SslPolicyErrors.None)
        {
            logger.LogDebug("SSL Policy Errors: None");
            return true;
        }

        if (cert is null)
        {
            logger.LogDebug("Certificate is null");
            return false;
        }

        if (cert.Thumbprint.Equals(options.Value.Thumbprint, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("Certificate thumbprint matches");
            return true;
        }

        // Log a less cryptic error than SSL could not be established.
        // Include actual in case it's the annual renew.
        logger.LogError(
            LogEvents.CertificateThumbprintMismatch,
            "Certificate thumbprint mismatch: Expected {Expected}, Actual {Actual}",
            options.Value.Thumbprint,
            cert.Thumbprint
        );

        notifyQueue.Enqueue(new ThumbprintAlert(options.Value.Thumbprint, cert.Thumbprint));

        return false;
    }
}