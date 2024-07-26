using Microsoft.Extensions.Logging.Testing;

using System.IO.Abstractions.TestingHelpers;

namespace Anar.Services.Locator.Tests;

public class LocatorTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly FakeLogger<Locator> _logger;

    public LocatorTests()
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                @"C:\layout.json",
                new MockFileData(TestData.LayoutFileJSON)
            },
            {
                @"C:\corrupt.json",
                new MockFileData(TestData.CorruptFileJSON)
            }
        });

        _logger = new();
    }

    [Fact]
    public void Locator_WhenFileIsGood_CreatesCollection()
    {
        // Arrange
        var options = new LocatorOptions { LayoutFile = @"C:\layout.json" };
        var locator = new Locator(_fileSystem, options, _logger);

        // Assert
        Assert.Equal(TestData.LayoutFileLocations, locator.Locations);
        Assert.Matches("Loaded 3 locations", _logger.LatestRecord.Message);
        // Assert that the same instance is returned
        Assert.Same(locator.Locations, locator.Locations);
    }

    [Fact]
    public void Locator_WhenFileNotFound_CreatesEmptyCollection()
    {
        // Arrange
        var options = new LocatorOptions { LayoutFile = @"C:\missing.json" };
        var locator = new Locator(_fileSystem, options, _logger);

        // Act
        Assert.Empty(locator.Locations);
        Assert.Matches("does not exist", _logger.LatestRecord.Message);
    }

    [Fact]
    public void Locator_WhenFileIsCorrupt_CreatesEmptyCollection()
    {
        // Arrange
        var options = new LocatorOptions { LayoutFile = @"C:\corrupt.json" };
        var locator = new Locator(_fileSystem, options, _logger);

        // Assert
        Assert.Empty(locator.Locations);
        Assert.Matches("Failed to parse", _logger.LatestRecord.Message);
    }

    [Fact]
    public void Locator_WhenFileIsNotSet_CreatesEmptyCollection()
    {
        // Arrange
        var options = new LocatorOptions { LayoutFile = "" };
        var locator = new Locator(_fileSystem, options, _logger);

        // Assert
        Assert.Empty(locator.Locations);
        Assert.Matches("No layout file specified", _logger.LatestRecord.Message);
    }
}

