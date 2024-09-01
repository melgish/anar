using System.ComponentModel.DataAnnotations;

namespace Anar.Services.Notify;

internal sealed record NotifyOptions
{
    /// <summary>
    /// the URI to send the notification to. Includes topic
    /// </summary>
    [Required]
    public Uri Uri { get; init; } = default!;

    /// <summary>
    /// the token to use for authentication to NTFY
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// The interval to poll the queue for new messages to send.
    /// </summary>
    [Required]
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The interval to wait before repeating a message.
    /// </summary>
    [Required]
    public TimeSpan SpamInterval { get; init; } = TimeSpan.FromDays(1);
}