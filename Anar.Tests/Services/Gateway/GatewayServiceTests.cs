namespace Anar.Tests.Services.Gateway;

using Anar.Services;
using Anar.Services.Gateway;
using Anar.Services.Notify;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Moq.Protected;
using System.Net;
using System.Security.Authentication;

public class GatewayServiceTests : IHttpClientFactory
{
    private readonly FakeLogger<GatewayService> _fakeLogger = new();
    private readonly Mock<HttpMessageHandler> _mockHandler = new();
    private readonly NotifyQueue _notifyQueue = new();

    public HttpClient CreateClient(string name)
        => new(_mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/api/v1/production/inverters"),
        };

    [Fact]
    public async Task GetInvertersAsync_WhenRequestReturnsData_ReturnsInverters()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{\"SerialNumber\": \"12345\"}]")
            })
            .Verifiable();

        // Act
        var client = new GatewayService(this, _fakeLogger, _notifyQueue);
        var result = await client.GetInvertersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("12345", result.First().SerialNumber);
    }

    [Fact]
    public async Task GetInvertersAsync_WhenRequestReturnsNull_ReturnsEmpty()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            })
            .Verifiable();

        // Act
        var client = new GatewayService(this, _fakeLogger, _notifyQueue);
        var result = await client.GetInvertersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInvertersAsync_WhenRequestFails401_ReturnsEmpty()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized))
            .Verifiable();

        // Act
        var client = new GatewayService(this, _fakeLogger, _notifyQueue);
        var result = await client.GetInvertersAsync();

        // Assert
        Assert.Empty(result);
        Assert.Equal(LogEvents.GetInvertersAuthorizationError, _fakeLogger.LatestRecord.Id);
        Assert.True(_notifyQueue.TryDequeue(out var alert));
        Assert.IsType<AuthenticationAlert>(alert);
    }

    [Fact]
    public async Task GetInvertersAsync_WhenRequestFailsSSL_ReturnsEmpty()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException(
                "SSL Handshake",
                new AuthenticationException("RemoteCertificateValidationCallback"),
                null))
            .Verifiable();

        // Act
        var client = new GatewayService(this, _fakeLogger, _notifyQueue);
        var result = await client.GetInvertersAsync();

        // Assert
        Assert.Empty(result);
        Assert.Equal(LogEvents.GetInvertersThumbprintError, _fakeLogger.LatestRecord.Id);
        Assert.False(_notifyQueue.TryDequeue(out var _));
    }

    [Fact]
    public async Task GetInvertersAsync_WhenRequestFailsOther_ReturnsEmpty()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new InvalidOperationException("Invalid Operation"))
            .Verifiable();

        // Act
        var client = new GatewayService(this, _fakeLogger, _notifyQueue);
        var result = await client.GetInvertersAsync();

        // Assert
        Assert.Empty(result);
        Assert.Equal(LogEvents.GetInvertersError, _fakeLogger.LatestRecord.Id);
        Assert.False(_notifyQueue.TryDequeue(out var _));
    }
}