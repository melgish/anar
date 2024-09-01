namespace Anar.Tests.Services.Notify;

using Anar.Services.Notify;
using Microsoft.Extensions.Options;

public class SpamFilterTests
{
    class TestTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = System.GetUtcNow();
        public override DateTimeOffset GetUtcNow() => Now;

        public void SetTwoDaysAgo()
            => Now = System.GetUtcNow().Subtract(TimeSpan.FromDays(2));

        public void SetNow()
            => Now = System.GetUtcNow();
    }
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
        spamFilter.IsOkToSend(new Alert());
        _timeProvider.SetNow();

        // Act
        spamFilter.IsOkToSend(new Alert());
        spamFilter.FlushExpired();

        // Assert
        Assert.Equal(1, spamFilter.Count);
    }
}