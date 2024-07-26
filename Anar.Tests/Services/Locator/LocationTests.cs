namespace Anar.Services.Locator.Tests;

public class LocationTests
{
    [Fact]
    public void Facing_ReturnsExpectedDirection()
    {
        // Assert
        Assert.Equal("north", new Location("SN123", "Array 1", 0).Facing);
        Assert.Equal("south", new Location("SN123", "Array 1", 180).Facing);
        Assert.Equal("east", new Location("SN123", "Array 1", 90).Facing);
        Assert.Equal("west", new Location("SN123", "Array 1", 270).Facing);
        Assert.Equal("unknown", new Location("SN123", "Array 1", 15).Facing);
    }
}