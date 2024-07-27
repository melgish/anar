using Anar.Services.Gateway;
using Anar.Services.Locator;
using Anar.Services.Worker;

namespace Anar.Tests.Services.Worker;

public sealed class ReadingTests
{
    [Fact]
    public void ToPointData_WhenLocationIsKnown_ShouldReturnPointData()
    {
        // Arrange
        var location = new Location("123", "Array 1", 180);
        var inverter = new Inverter
        {
            SerialNumber = "123",
            LastReportDate = 17123450,
            LastReportWatts = 1000
        };

        var reading = new Reading(inverter, location);

        // Act
        var result = reading.ToPointData().ToLineProtocol();

        // Assert
        Assert.Equal(@"inverter,arrayName=Array\ 1,facing=south,serialNumber=123 watts=1000i 17123450", result);
    }

    [Fact]
    public void ToPointData_WhenLocationIsUnknown_ShouldReturnPointData()
    {
        // Arrange
        var inverter = new Inverter
        {
            SerialNumber = "123",
            LastReportDate = 17123450,
            LastReportWatts = 1000
        };
        var reading = new Reading(inverter, null);

        // Act
        var result = reading.ToPointData().ToLineProtocol();

        // Assert
        Assert.Equal(@"inverter,serialNumber=123 watts=1000i 17123450", result);
    }
}