using System.Net;

using Anar.Services.Gateway;
using Anar.Services.Locator;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;



namespace Anar.Tests.Services.Gateway;

public class GatewayServiceTests : IHttpClientFactory
{
    private readonly FakeLogger<GatewayService> _fakeLogger;
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public GatewayServiceTests()
    {
        _fakeLogger = new FakeLogger<GatewayService>();
        _mockHandler = new Mock<HttpMessageHandler>();
    }

    public HttpClient CreateClient(string name)
        => new(_mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/api/v1/production/inverters"),
        };

    [Fact]
    public async Task GetInvertersAsync_Success_ReturnsInverters()
    {
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

        var client = new GatewayService(this, _fakeLogger);
        var result = await client.GetInvertersAsync();

        Assert.Single(result);
        Assert.Equal("12345", result.First().SerialNumber);
    }

    [Fact]
    public async Task GetInvertersAsync_Error_ReturnsEmpty()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            })
            .Verifiable();

        var client = new GatewayService(this, _fakeLogger);
        var result = await client.GetInvertersAsync();

        Assert.Empty(result);
        Assert.Matches("get inverters", _fakeLogger.LatestRecord.Message);
    }

}