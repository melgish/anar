namespace Anar.Services.Locator.Tests;

public class LayoutDTOTests
{
    [Fact]
    public void ToLocations_ConvertsAllLocations()
    {
        var source = new LayoutDTO
        {
            Arrays =
            [
                new()
                {
                    ArrayName = "Array 1",
                    Azimuth = 180,
                    Modules =
                    [
                       new() { Inverter = new () { SerialNumber = "SN123" } },
                       new() { Inverter = new () { SerialNumber = "SN124" } }
                    ]
                },
                new()
                {
                    ArrayName = "Array 2",
                    Azimuth = 90,
                    Modules =
                    [
                       new() { Inverter = new () { SerialNumber = "SN125" } },
                       new() { Inverter = new () { SerialNumber = "SN126" } }
                    ]
                }
            ]
        };

        var result = source.ToLocations().ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(new("SN123", "Array 1", 180), result[0]);
        Assert.Equal(new("SN124", "Array 1", 180), result[1]);
        Assert.Equal(new("SN125", "Array 2", 90), result[2]);
        Assert.Equal(new("SN126", "Array 2", 90), result[3]);
    }
}