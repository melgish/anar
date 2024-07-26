using Anar.Services.Gateway;
using Anar.Services.Locator;
using Anar.Services.Worker;

namespace Anar.Tests.Services.Worker;

public sealed class TotalsTests
{
    [Fact]
    public void ToPointData_ShouldReturnPointData()
    {
        // Act
        var result = TestData.Totals.ToPointData().ToLineProtocol();

        // Assert
        Assert.Equal(TestData.TotalsLine, result);
    }

    [Fact]
    public void ToPointData_WhenLocationIsUnknown_ShouldReturnPointData()
    {
        // Arrange
        var totals = Totals.FromReadings(TestData.Readings);

        // Assert
        Assert.Equal(TestData.Totals, totals);
    }
}