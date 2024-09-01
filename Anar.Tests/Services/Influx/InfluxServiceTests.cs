namespace Anar.Tests.Services.Influx;

using Anar.Services;
using Anar.Services.Influx;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using System;

public sealed class InfluxServiceTests : IInfluxDbClientFactory
{
    private readonly FakeLogger<InfluxService> _logger = new();
    private readonly Mock<IInfluxDBClient> _mockClient = new();
    private readonly Mock<IWriteApiAsync> _mockWriteApiAsync = new();

    IInfluxDBClient IInfluxDbClientFactory.CreateClient(Uri uri, string token)
    {
        return _mockClient.Object;
    }

    private InfluxService Setup()
    {
        _mockClient.Setup(x => x.GetWriteApiAsync(null)).Returns(_mockWriteApiAsync.Object);
        _mockWriteApiAsync.Setup(x => x.WritePointsAsync(
            It.IsAny<List<PointData>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask);
        return new InfluxService(
            this,
            Options.Create<InfluxOptions>(new()),
            _logger
        );
    }

    [Fact]
    public async Task WritePointsAsync_WhenSuccessful_ShouldWritePoints()
    {
        // Arrange
        var influxService = Setup();

        // Act
        await influxService.WritePointsAsync([]);

        // Assert
        Assert.NotNull(influxService);
        _mockClient.Verify(x => x.GetWriteApiAsync(null), Times.Once);
        _mockWriteApiAsync.Verify(x => x.WritePointsAsync(
            It.IsAny<List<PointData>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task WritePointsAsync_WhenUnsuccessful_ShouldLogWarning()
    {
        // Arrange
        var influxService = Setup();
        _mockWriteApiAsync.Setup(x => x.WritePointsAsync(
            It.IsAny<List<PointData>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("Kaboom!"));

        // Act
        await influxService.WritePointsAsync([]);

        // Assert
        Assert.NotNull(influxService);
        _mockClient.Verify(x => x.GetWriteApiAsync(null), Times.Once);
        _mockWriteApiAsync.Verify(x => x.WritePointsAsync(
            It.IsAny<List<PointData>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        Assert.Equal(LogEvents.InfluxDbWriteError, _logger.LatestRecord.Id);
    }
}