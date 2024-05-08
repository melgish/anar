using System.ComponentModel.DataAnnotations;

namespace Anar.Services;

internal sealed class InfluxOptions
{
    [Required]
    public Uri Uri { get; set; } = default!;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string? Bucket { get; set; } = null;

    [Required]
    public string? Organization { get; set; } = null;
}