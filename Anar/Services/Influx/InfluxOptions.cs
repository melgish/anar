using System.ComponentModel.DataAnnotations;

namespace Anar.Services;

internal sealed class InfluxOptions
{
    /// <summary>
    /// Influx DB bucket for data writing.
    /// </summary>
    [Required]
    public string? Bucket { get; set; } = null;

    /// <summary>
    /// Influx DB organization for data writing.
    /// </summary>
    [Required]
    public string? Organization { get; set; } = null;

    /// <summary>
    /// Influx DB token.  Token must grant write access to named bucket.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Influx DB URL.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = default!;
}