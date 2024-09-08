namespace Anar.Tests.Services.Notify;

using Anar.Services;
using Anar.Services.Notify;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Moq.Protected;
using System.Net;

public sealed class NotifyProcessorTests : IHttpClientFactory
{
    private readonly Mock<HttpMessageHandler> _mockHandler = new();
    private readonly NotifyQueue _notifyQueue = new();
    private readonly FakeLogger<NotifyProcessor> _fakeLogger = new();
    private readonly Mock<ISpamFilter> _spamFilter = new();

    HttpClient IHttpClientFactory.CreateClient(string name)
        => new(_mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/api/v1/notify/alerts"),
        };

    private NotifyProcessor CreateProcessor(HttpStatusCode statusCode)
    {
        _notifyQueue.Enqueue(new SimpleAlert("Test message1"));
        _notifyQueue.Enqueue(new GatewayTokenExpirationAlert(DateTime.UtcNow));

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

        return new NotifyProcessor(
            this,
            _notifyQueue,
            _spamFilter.Object,
            _fakeLogger
        );
    }

    [Fact]
    public async Task ProcessNotificationsAsync_WhenShuttingDown_DoesNotSend()
    {
        // Arrange
        var service = CreateProcessor(HttpStatusCode.OK);

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
        var service = CreateProcessor(HttpStatusCode.OK);

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
        var service = CreateProcessor(HttpStatusCode.NotFound);

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