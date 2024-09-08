namespace Anar.Test.Services.Notify;

using Anar.Services;
using Anar.Services.Notify;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;

public class NotifyWorkerTests
{
    private readonly FakeLogger<NotifyWorker> _logger = new();
    private readonly Mock<INotifyProcessor> _processor = new();

    private IOptions<NotifyOptions> CreateOptions(TimeSpan pollingInterval)
        => Options.Create(new NotifyOptions { PollingInterval = pollingInterval });

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_DoesNotProcessNotifications()
    {
        // Arrange
        var options = CreateOptions(TimeSpan.FromMinutes(1));


        // Act
        await new NotifyWorker(_processor.Object, options, _logger)
            .InvokeExecuteAsync(new(true));

        // Assert
        _processor.Verify(x => x.ProcessNotificationsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionIsThrown_LogsError()
    {
        // Arrange
        var options = CreateOptions(TimeSpan.FromMinutes(1));
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        _processor
             .Setup(x => x.ProcessNotificationsAsync(It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception("Test exception"));

        // Act
        await new NotifyWorker(_processor.Object, options, _logger)
            .InvokeExecuteAsync(tokenSource.Token);

        // Assert
        Assert.Equal(LogEvents.NotifyWorkerError, _logger.LatestRecord.Id);
    }

    [Fact]
    public async Task ExecuteAsync_CallsService()
    {
        // Arrange
        var options = CreateOptions(TimeSpan.FromMilliseconds(200));
        var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        // Act
        await new NotifyWorker(_processor.Object, options, _logger)
            .InvokeExecuteAsync(tokenSource.Token);

        // Assert
        _processor.Verify(x => x.ProcessNotificationsAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

}