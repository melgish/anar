using Microsoft.Extensions.Logging.Testing;

using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;
using MyWorker = Anar.Services.Worker.Worker;
using Anar.Services.Worker;

using Moq;
using InfluxDB.Client.Writes;

namespace Anar.Tests.Services.Worker;

public sealed class WorkerTests
{
    readonly Mock<ILocator> locator = new();
    readonly Mock<IGatewayClient> gatewayClient = new();
    readonly WorkerOptions options = new() { Interval = TimeSpan.FromSeconds(15) };

    private void Setup(IList<Inverter> results, IList<Location> locations)
    {
        locator.Setup(x => x.Locations).Returns(locations);
        gatewayClient.Setup(x =>
            x.GetInvertersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);
    }

    [Fact]
    public async Task ProcessData_WhenNoInverters_DoesNotWriteData()
    {
        // Arrange
        Setup([], []);

        var influxService = new Mock<IInfluxService>();
        var logger = new FakeLogger<MyWorker>();

        var worker = new MyWorker(
            options,
            locator.Object,
            gatewayClient.Object,
            influxService.Object,
            logger);

        // Act
        await worker.ProcessData(CancellationToken.None);

        // Assert
        influxService.Verify(x =>
            x.WritePointsAsync(
                It.IsAny<IEnumerable<PointData>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [Fact]
    public async Task ProcessData_WhenInverters_WritesData()
    {
        Setup(TestData.InverterResponse, []);

        // Arrange
        var options = new WorkerOptions { Interval = TimeSpan.FromSeconds(5) };

        var influxService = new Mock<IInfluxService>();
        var logger = new FakeLogger<MyWorker>();

        var worker = new MyWorker(
            options,
            locator.Object,
            gatewayClient.Object,
            influxService.Object,
            logger);

        // Act
        await worker.ProcessData(CancellationToken.None);

        // Assert
        influxService.Verify(x =>
            x.WritePointsAsync(
                It.IsAny<IEnumerable<PointData>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }
}