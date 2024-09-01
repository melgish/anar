namespace Anar.Tests.Services.Notify;

using Anar.Services;
using Anar.Services.Notify;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

public sealed class NotifyServiceTests : IHttpClientFactory, IOptions<NotifyOptions>
{
    private NotifyOptions Options { get; set; } = new()
    {
        PollingInterval = TimeSpan.FromMilliseconds(100)
    };
    private readonly Mock<HttpMessageHandler> _mockHandler = new();
    private readonly NotifyQueue _notifyQueue = new();
    private readonly FakeLogger<NotifyService> _fakeLogger = new();
    private readonly Mock<ISpamFilter> _spamFilter = new();

    NotifyOptions IOptions<NotifyOptions>.Value => Options;

    HttpClient IHttpClientFactory.CreateClient(string name)
        => new(_mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/api/v1/notify/alerts"),
        };

    private NotifyService Setup(HttpStatusCode statusCode)
    {
        _notifyQueue.Enqueue(new SimpleAlert("Test message1"));
        _notifyQueue.Enqueue(new SimpleAlert("Test message2"));


        _spamFilter
            .SetupSequence(x => x.IsOkToSend(It.IsAny<Alert>()))
            .Returns(true)
            .Returns(false);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{}")
            });

        return new NotifyService(
            this, _notifyQueue,
            this,
            _spamFilter.Object,
            _fakeLogger
        );
    }

    [Fact]
    public async Task ProcessNotificationsAsync_WhenShuttingDown_DoesNotSend()
    {
        // Arrange
        var service = Setup(HttpStatusCode.OK);

        // Act
        await service.ProcessNotificationsAsync(new(true));

        // Assert
        Assert.Equal(2, _notifyQueue.Count);
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ProcessNotificationsAsync_WhenOkToSend_Sends()
    {
        // Arrange
        var service = Setup(HttpStatusCode.OK);

        // Act
        await service.ProcessNotificationsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(_notifyQueue);
        Assert.Equal(LogEvents.NotifySending, _fakeLogger.LatestRecord.Id);
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ProcessNotificationsAsync_WhenRequestFails_LogsWarning()
    {
        // Arrange
        var service = Setup(HttpStatusCode.NotFound);

        // Act
        await service.ProcessNotificationsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(_notifyQueue);
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

}