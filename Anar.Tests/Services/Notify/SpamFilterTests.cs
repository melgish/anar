namespace Anar.Tests.Services.Notify;

using Anar.Services.Notify;
using Microsoft.Extensions.Options;

public class SpamFilterTests
{
    private readonly TestTimeProvider _timeProvider = new();
    private readonly IOptions<NotifyOptions> _options = Options.Create<NotifyOptions>(new());

    private SpamFilter Setup()
    {
        return new SpamFilter(_timeProvider, _options);
    }

    [Fact]
    public void IsOkToSend_ReturnsTrue_WhenAlertIsNotInDictionary()
    {
        // Arrange
        var spamFilter = Setup();

        // Act
        var result = spamFilter.IsOkToSend(new SimpleAlert("test"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOkToSend_ReturnsFalse_WhenAlertIsInDictionary()
    {
        // Arrange
        var spamFilter = Setup();

        // Act
        spamFilter.IsOkToSend(new SimpleAlert("test"));
        var result = spamFilter.IsOkToSend(new SimpleAlert("test"));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOkToSend_ReturnsTrue_WhenAlertIsInDictionaryButExpired()
    {
        // Arrange
        var spamFilter = Setup();
        _timeProvider.SetTwoDaysAgo();
        spamFilter.IsOkToSend(new SimpleAlert("test"));
        _timeProvider.SetNow();

        // Act
        var result = spamFilter.IsOkToSend(new SimpleAlert("test"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FlushExpired_RemovesExpiredAlerts()
    {
        // Arrange
        var spamFilter = Setup();
        _timeProvider.SetTwoDaysAgo();
        spamFilter.IsOkToSend(new SimpleAlert("test"));
        spamFilter.IsOkToSend(new ThumbprintAlert("expected", "actual"));
        spamFilter.IsOkToSend(new SimpleAlert("other"));
        _timeProvider.SetNow();

        // Act
        spamFilter.IsOkToSend(new SimpleAlert("other"));
        spamFilter.FlushExpired();

        // Assert
        Assert.Equal(1, spamFilter.Count);
    }
}