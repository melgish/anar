namespace Anar.Services.Locator.Tests;

public class LayoutDTOTests
{
    [Fact]
    public void ToLocations_ConvertsAllLocations()
    {
        var source = TestData.LayoutFileData;

        var result = source.ToLocations().ToList();

        // Assert
        Assert.Equal(TestData.LayoutFileLocations, result);
    }
}