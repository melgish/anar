using System.ComponentModel.DataAnnotations;

namespace Anar.Services;

internal sealed class InfluxOptions
{
    /// <summary>
    /// Influx DB bucket for data writing.
    /// </summary>
    [Required]
    public string? Bucket { get; init; } = null;

    /// <summary>
    /// Influx DB organization for data writing.
    /// </summary>
    [Required]
    public string? Organization { get; init; } = null;

    /// <summary>
    /// Influx DB token.  Token must grant write access to named bucket.
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Influx DB URL.
    /// </summary>
    [Required]
    public Uri Uri { get; init; } = default!;
}