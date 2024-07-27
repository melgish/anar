// spell-checker: words Enphase
using System.ComponentModel.DataAnnotations;

namespace Anar.Services.Gateway;

internal sealed class GatewayOptions
{
    /// <summary>
    /// Thumbprint of the self-signed certificate in the gateway.
    /// </summary>
    [Required]
    public string Thumbprint { get; init; } = string.Empty;

    /// <summary>
    /// Path to the endpoint on the gateway.
    /// </summary>
    [Required]
    public string RequestPath { get; init; } = "api/v1/production/inverters";

    /// <summary>
    /// Token to use for request authentication.
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Base URI to Enphase gateway.
    /// </summary>
    [Required]
    public Uri Uri { get; init; } = null!;
}