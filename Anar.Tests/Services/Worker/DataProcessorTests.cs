namespace Anar.Tests.Services.Worker;

using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;
using Anar.Services.Worker;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging.Testing;
using Moq;

public sealed class DataProcessorTests
{
    readonly Mock<ILocatorService> _locator = new();
    readonly Mock<IGatewayService> _gateway = new();
    readonly Mock<IInfluxService> _influx = new();
    readonly FakeLogger<DataProcessor> _logger = new();

    private void Setup(IList<Inverter> results, IList<Location> locations)
    {
        _locator.Setup(x => x.Locations).Returns(locations);
        _gateway.Setup(x => x.GetInvertersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);
    }

    [Fact]
    public async Task ProcessData_WhenNoInverters_DoesNotWriteData()
    {
        // Arrange
        Setup([], []);

        // Act
        await new DataProcessor(
            _locator.Object,
            _gateway.Object,
            _influx.Object,
            _logger).ProcessDataAsync(CancellationToken.None);

        // Assert
        _influx.Verify(x => x.WritePointsAsync(
            It.IsAny<List<PointData>>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task ProcessData_WhenInverters_WritesData()
    {
        // Arrange
        Setup(TestData.InverterResponse, []);

        // Act
        await new DataProcessor(
            _locator.Object,
            _gateway.Object,
            _influx.Object,
            _logger).ProcessDataAsync(CancellationToken.None);

        // Assert
        _influx.Verify(x =>
            x.WritePointsAsync(
                It.IsAny<List<PointData>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }
}