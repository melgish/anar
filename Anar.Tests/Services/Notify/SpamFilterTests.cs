namespace Anar.Tests.Services.Notify;

using Anar.Services.Notify;
using Microsoft.Extensions.Options;

public class SpamFilterTests
{
    class TestTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = System.GetUtcNow();
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void IsOkToSend_ReturnsTrue_WhenAlertIsNotInDictionary()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var options = Options.Create(new NotifyOptions());
        var spamFilter = new SpamFilter(timeProvider, options);

        // Act
        var result = spamFilter.IsOkToSend(new SimpleAlert("test"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOkToSend_ReturnsFalse_WhenAlertIsInDictionary()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var options = Options.Create(new NotifyOptions());
        var spamFilter = new SpamFilter(timeProvider, options);

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
        var timeProvider = new TestTimeProvider();
        var options = Options.Create(new NotifyOptions());
        var spamFilter = new SpamFilter(timeProvider, options);

        // Act
        timeProvider.Now = TimeProvider.System.GetUtcNow().Subtract(TimeSpan.FromDays(2));
        spamFilter.IsOkToSend(new SimpleAlert("test"));
        timeProvider.Now = TimeProvider.System.GetUtcNow();
        var result = spamFilter.IsOkToSend(new SimpleAlert("test"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FlushExpired_RemovesExpiredAlerts()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var options = Options.Create(new NotifyOptions());
        var spamFilter = new SpamFilter(timeProvider, options);

        // Act
        timeProvider.Now = TimeProvider.System.GetUtcNow().Subtract(TimeSpan.FromDays(2));
        spamFilter.IsOkToSend(new SimpleAlert("test"));
        spamFilter.IsOkToSend(new ThumbprintAlert("expected", "actual"));
        spamFilter.IsOkToSend(new Alert());
        timeProvider.Now = TimeProvider.System.GetUtcNow();
        spamFilter.IsOkToSend(new Alert());

        spamFilter.FlushExpired();

        // Assert
        Assert.Equal(1, spamFilter.Count);
    }
}
