using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Anar.Services.Notify;

namespace Anar.Services.Gateway;

internal sealed class GatewayThumbprintValidator(
    IOptions<GatewayOptions> options,
    ILogger<GatewayThumbprintValidator> logger,
    INotifyQueue notifyQueue
)
{
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