using Anar.Services;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using System.Net;

namespace Anar.Tests.Services;

public class GatewayClientTests
{
    private readonly FakeLogger<GatewayClient> _fakeLogger;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<GatewayOptions>> _mockOptions;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly GatewayOptions _options;

    public GatewayClientTests()
    {
        _options = new GatewayOptions
        {
            Uri = new Uri("http://localhost/"),
            Token = "token",
            Interval = TimeSpan.FromSeconds(10),
            Layout = [
                new Location { SerialNumber = "12345", Azimuth = 0, ArrayName = "Array 1" },
            ]
        };

        _fakeLogger = new FakeLogger<GatewayClient>();
        _mockOptions = new Mock<IOptions<GatewayOptions>>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHandler.Object) {
            BaseAddress = _options.Uri,
        };

        _mockOptions.Setup(o => o.Value).Returns(_options);
    }

    private GatewayClient CreateGatewayClient()
    {
        return new GatewayClient(_fakeLogger, _mockOptions.Object, _httpClient);
    }

    [Fact]
    public void Interval_ShouldReturnOptionsValue()
    {
        var client = CreateGatewayClient();

        Assert.Equal(_options.Interval, client.Interval);
    }

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

        var client = CreateGatewayClient();
        var result = await client.GetInvertersAsync();

        Assert.Single(result);
        Assert.Equal("12345", result.First().SerialNumber);
        Assert.NotNull(result.First().Location);
    }

    [Fact]
    public async Task GetInvertersAsync_Success_ReturnsEmpty()
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

        var client = CreateGatewayClient();
        var result = await client.GetInvertersAsync();

        Assert.Empty(result);
        Assert.Matches("get inverters", _fakeLogger.LatestRecord.Message);
    }
}