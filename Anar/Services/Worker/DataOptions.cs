using System.ComponentModel.DataAnnotations;

namespace Anar.Services.Worker;

internal sealed class DataOptions
{
    /// <summary>
    /// Polling interval
    /// </summary>
    [Required]
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromMinutes(5);
}