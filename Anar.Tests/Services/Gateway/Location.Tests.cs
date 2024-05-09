using Anar.Services;

namespace Anar.Tests.Services;

public class LocationTest
{
    [Fact]
    public void Facing_Should_Return_A_Direction()
    {
        Location north = new(){ Azimuth = 0 };
        Assert.Equal("north", north.Facing);
        Location east = new() { Azimuth = 90 };
        Assert.Equal("east", east.Facing);
        Location south = new() { Azimuth = 180 };
        Assert.Equal("south", south.Facing);
        Location west = new() { Azimuth = 270 };
        Assert.Equal("west", west.Facing);
        Location unknown = new() { Azimuth = 123 };
        Assert.Equal("unknown", unknown.Facing);
    }
}