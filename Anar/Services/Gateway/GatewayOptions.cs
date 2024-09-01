// spell-checker: words Enphase
namespace Anar.Services.Gateway;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

internal sealed partial class GatewayOptions
{
    [ExcludeFromCodeCoverage]
    [GeneratedRegex("[^0-9A-F]", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex NonHexDigitsRegex();

    /// <summary>
    /// Backing store for Thumbprint property.
    /// </summary>
    private string _thumbprint = string.Empty;

    /// <summary>
    /// Thumbprint of the self-signed certificate in the gateway.
    /// </summary>
    [Required]
    public string Thumbprint
    {
        get => _thumbprint;
        // Allow thumbprint to contain punctuation AB:CD:EF
        init => _thumbprint = NonHexDigitsRegex().Replace(value, string.Empty);
    }

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