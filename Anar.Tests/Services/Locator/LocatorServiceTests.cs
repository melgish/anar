namespace Anar.Services.Locator.Tests;

using Anar.Services.Locator;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using System.IO.Abstractions.TestingHelpers;

public class LocatorTests
{
    private readonly MockFileSystem _fileSystem = new(
        new Dictionary<string, MockFileData>
        {
            {
                @"C:\layout.json",
                new MockFileData(TestData.LayoutFileJSON)
            },
            {
                @"C:\corrupt.json",
                new MockFileData(TestData.CorruptFileJSON)
            },
            {
                @"C:\null.json",
                new MockFileData("null")
            }
        }
    );

    private readonly FakeLogger<LocatorService> _logger = new();

    private LocatorService Setup(string layoutFile)
    {
        var options = Options.Create<LocatorOptions>(new() { LayoutFile = layoutFile });
        return new LocatorService(_fileSystem, options, _logger);
    }

    [Fact]
    public void Locator_WhenFileIsGood_CreatesCollection()
    {
        // Act
        var locator = Setup(@"C:\layout.json");

        // Assert
        Assert.Equal(TestData.LayoutFileLocations, locator.Locations);
        Assert.Matches("Loaded 3 locations", _logger.LatestRecord.Message);
        // Assert that the same instance is returned
        Assert.Same(locator.Locations, locator.Locations);
    }

    [Fact]
    public void Locator_WhenFileContainsNull_CreatesEmptyCollection()
    {
        // Act
        var locator = Setup(@"C:\null.json");

        // Assert
        Assert.Empty(locator.Locations);
    }

    [Fact]
    public void Locator_WhenFileNotFound_CreatesEmptyCollection()
    {
        // Act
        var locator = Setup(@"C:\missing.json");

        // Assert
        Assert.Empty(locator.Locations);
        Assert.Equal(LogEvents.LayoutFileNotFound, _logger.LatestRecord.Id);
    }

    [Fact]
    public void Locator_WhenFileIsCorrupt_CreatesEmptyCollection()
    {
        // Act
        var locator = Setup(@"C:\corrupt.json");

        // Assert
        Assert.Empty(locator.Locations);
        Assert.Equal(LogEvents.LayoutFileFormatError, _logger.LatestRecord.Id);
    }

    [Fact]
    public void Locator_WhenFileIsNotSet_CreatesEmptyCollection()
    {
        // Act
        var locator = Setup(string.Empty);

        // Assert
        Assert.Empty(locator.Locations);
        Assert.Equal(LogEvents.NoLayoutFile, _logger.LatestRecord.Id);
    }
}