using Anar.Services;

namespace Anar.Tests.Services;

public sealed class DomainExtensionsTests
{
    [Fact]
    public void ToPointData_WhenLocationIsNotNull_ShouldReturnPointData()
    {
        var inverter = new Inverter
        {
            SerialNumber = "123",
            LastReportDate = 123456789,
            LastReportWatts = 123,
            Location = new()
            {
                ArrayName = "array",
                Azimuth = 90,
                SerialNumber = "123",
            }
        };

        var point = inverter.ToPointData();

        Assert.Equal("inverter,arrayName=array,facing=east,serialNumber=123 watts=123i 123456789", point.ToLineProtocol());
    }

    [Fact]
    public void ToPointData_WhenLocationNull_ShouldReturnPointData()
    {
        var inverter = new Inverter
        {
            SerialNumber = "123",
            LastReportDate = 123456789,
            LastReportWatts = 123,
        };

        var point = inverter.ToPointData();

        Assert.Equal("inverter,serialNumber=123 watts=123i 123456789", point.ToLineProtocol());
    }
}