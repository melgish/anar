namespace Anar.Services.Notify;

using System.Collections.Concurrent;

using Microsoft.Extensions.Options;

internal interface ISpamFilter
{
    /// <summary>
    /// Remove all expired entries.
    /// Called at the start of notification processing to keep the dictionary
    /// from growing indefinitely.
    /// </summary>
    void FlushExpired();

    /// <summary>
    /// Check if the alert is not in the dictionary or if the last time it was
    /// sent is older than the configured spam interval.
    /// </summary>
    /// <param name="alert">Alert instance to test</param>
    /// <returns>True if alert can be sent without being an nuisance</returns>
    bool IsOkToSend(Alert alert);
}

/// <summary>
/// Implements the spam filter for the notification service.
/// </summary>
/// <param name="timeProvider"></param>
/// <param name="options"></param>
internal sealed class SpamFilter(
    TimeProvider timeProvider,
    IOptions<NotifyOptions> options
) : ISpamFilter
{
    private readonly ConcurrentDictionary<Alert, long> _alerts = new();
    private readonly long _spamTicks = options.Value.SpamInterval.Ticks;

    /// <summary>
    /// Gets current size of the dictionary.  Exposed for testing.
    /// </summary>
    public int Count => _alerts.Count;

    /// <summary>
    /// Remove all expired entries.
    /// Called at the start of notification processing to keep the dictionary
    /// from growing indefinitely.
    /// </summary>
    public void FlushExpired()
    {
        var minCreated = timeProvider.GetUtcNow().Ticks - _spamTicks;
        foreach (var (key, created) in _alerts)
        {
            if (created < minCreated)
            {
                _alerts.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Check if the alert is not in the dictionary or if the last time it was
    /// sent is older than the configured spam interval.
    /// </summary>
    /// <param name="alert">Alert instance to test</param>
    /// <returns>True if alert can be sent without being an nuisance</returns>
    public bool IsOkToSend(Alert alert)
    {
        var created = timeProvider.GetUtcNow().Ticks;
        var minCreated = created - _spamTicks;
        if (_alerts.TryGetValue(alert, out var lastSent))
        {
            if (lastSent >= minCreated)
            {
                return false;
            }
        }

        _alerts[alert] = created;
        return true;
    }
}