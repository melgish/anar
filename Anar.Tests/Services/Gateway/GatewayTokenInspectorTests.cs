namespace Anar.Tests.Services.Gateway;

using Anar.Services.Gateway;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

public class GatewayTokenInspectorTests
{
    private string CreateToken(object payload)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        var base64Payload = Convert.ToBase64String(payloadBytes);
        return $"header.{base64Payload}.signature";
    }

    private IOptions<GatewayOptions> CreateOptions(DateTimeOffset expirationDate)
    {
        // Create a fake token using the expiration date.
        var token = CreateToken(new
        {
            exp = expirationDate.ToUnixTimeSeconds()
        });
        return Options.Create<GatewayOptions>(new() { Token = token }); ;
    }


    [Fact]
    public void GetLifetimeRemaining_WhenTokenIsExpired_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var options = CreateOptions(now.AddDays(-1.1));
        var timeProvider = new TestTimeProvider { Now = now };
        var inspector = new GatewayTokenInspector(options, timeProvider);

        // Act
        var result = inspector.GetLifetimeRemaining();

        // Assert
        Assert.True(result.Days < 0);
    }

    [Fact]
    public void GetLifetimeRemaining_WhenTokenIsNotExpired_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var options = CreateOptions(now.AddDays(1.1));
        var timeProvider = new TestTimeProvider { Now = now };
        var inspector = new GatewayTokenInspector(options, timeProvider);

        // Act
        var result = inspector.GetLifetimeRemaining();

        // Assert
        Assert.True(result.Days > 0);
    }

    [Fact]
    public void GetLifetimeRemaining_WhenTokenIsNotValid_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var options = Options.Create<GatewayOptions>(new() { Token = "abc.def.gji" });
        var timeProvider = new TestTimeProvider { Now = now };
        var inspector = new GatewayTokenInspector(options, timeProvider);

        // Act
        var result = inspector.GetLifetimeRemaining();

        // Assert
        Assert.True(result.Days < 0);
    }

    [Fact]
    public void GetLifetimeRemaining_WhenTokenIsNullOrEmpty_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var options = Options.Create<GatewayOptions>(new() { Token = null! });
        var timeProvider = new TestTimeProvider { Now = now };
        var inspector = new GatewayTokenInspector(options, timeProvider);

        // Act
        var result = inspector.GetLifetimeRemaining();

        // Assert
        Assert.True(result.Days < 0);
    }

    [Fact]
    public void GetLifetimeRemaining_WhenTokenDoesNotHaveExp_ReturnsNegativeTimeSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var options = Options.Create<GatewayOptions>(new()
        {
            Token = CreateToken(new { iat = now.ToUnixTimeSeconds() })
        });
        var timeProvider = new TestTimeProvider { Now = now };
        var inspector = new GatewayTokenInspector(options, timeProvider);

        // Act
        var result = inspector.GetLifetimeRemaining();

        // Assert
        Assert.True(result.Days < 0);
    }
}