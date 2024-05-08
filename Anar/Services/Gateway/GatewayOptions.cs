// spell-checker: words Enphase
using System.ComponentModel.DataAnnotations;

namespace Anar.Services;

internal sealed class GatewayOptions
{
    /// <summary>
    /// Polling interval
    /// </summary>
    [Required]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Array of panel locations
    /// </summary>
    [Required]
    public Location[] Layout { get; set; } = Array.Empty<Location>();

    /// <summary>
    /// File containing array layout info.
    /// </summary>
    public string LayoutFile { get; set; } = string.Empty;

    /// <summary>
    /// Thumbprint of the self-signed certificate in the gateway.
    /// </summary>
    [Required]
    public string Thumbprint { get; set; } = string.Empty;

    /// <summary>
    /// Token to use for request authentication.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Base URI to Enphase gateway.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = null!;
}