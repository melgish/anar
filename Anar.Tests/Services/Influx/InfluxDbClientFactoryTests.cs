using Anar.Services.Influx;

namespace Anar.Tests.Services.Influx;

public sealed class InfluxDbClientFactoryTests
{
    [Fact]
    public void CreateClient_WhenCalled_ShouldReturnClient()
    {
        // Arrange
        var factory = new InfluxDbClientFactory();

        // Act
        var client = factory.CreateClient(new Uri("http://localhost:8086"), "token");

        // Assert
        Assert.NotNull(client);
    }
}