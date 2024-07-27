using System.Net;

using Anar.Services.Gateway;
using Anar.Services.Locator;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using MyGateway = Anar.Services.Gateway.Gateway;

namespace Anar.Tests.Services.Gateway;

public class GatewayClientTests
{
    private readonly FakeLogger<MyGateway> _fakeLogger;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<GatewayOptions>> _mockOptions;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly Mock<ILocator> _mockLocator;
    private readonly GatewayOptions _options;

    public GatewayClientTests()
    {
        _options = new GatewayOptions
        {
            Uri = new Uri("http://localhost/"),
            Token = "token",
            Thumbprint = "thumbprint",
        };

        _fakeLogger = new FakeLogger<MyGateway>();
        _mockOptions = new Mock<IOptions<GatewayOptions>>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockLocator = new Mock<ILocator>();
        _httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = _options.Uri,
        };

        _mockOptions.Setup(o => o.Value).Returns(_options);
    }

    private MyGateway CreateGatewayClient()
    {
        return new MyGateway(_httpClient, _mockOptions.Object, _fakeLogger);
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

        var client = CreateGatewayClient();
        var result = await client.GetInvertersAsync();

        Assert.Empty(result);
        Assert.Matches("get inverters", _fakeLogger.LatestRecord.Message);
    }
}