using System.ComponentModel.DataAnnotations;

namespace Anar.Services.Worker;

internal sealed class WorkerOptions
{
    /// <summary>
    /// Polling interval
    /// </summary>
    [Required]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
}