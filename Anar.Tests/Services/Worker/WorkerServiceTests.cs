using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;
using Anar.Services.Worker;

using InfluxDB.Client.Writes;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;

using MyWorker = Anar.Services.Worker.WorkerService;

namespace Anar.Tests.Services.Worker;

public sealed class WorkerServiceTests
{
    readonly Mock<ILocatorService> locator = new();
    readonly Mock<IGatewayService> gatewayClient = new();
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
            Options.Create(options),
            locator.Object,
            gatewayClient.Object,
            influxService.Object,
            logger);

        // Act
        await worker.ProcessData(CancellationToken.None);

        // Assert
        influxService.Verify(x =>
            x.WritePointsAsync(
                It.IsAny<List<PointData>>(),
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
            Options.Create(options),
            locator.Object,
            gatewayClient.Object,
            influxService.Object,
            logger);

        // Act
        await worker.ProcessData(CancellationToken.None);

        // Assert
        influxService.Verify(x =>
            x.WritePointsAsync(
                It.IsAny<List<PointData>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }
}