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
                    new MockFileData("""
                     {
                       "arrays": [
                         {
                           "label": "Array 1",
                           "azimuth": 180,
                           "modules": [
                             {   "inverter": { "serial_num": "SN123" } },
                             {   "inverter": { "serial_num": "SN124" } }
                           ]
                         },
                         {
                           "label": "Array 2",
                           "azimuth": 90,
                           "modules": [
                             {   "inverter": { "serial_num": "SN125" } }
                           ]
                         }
                       ]
                     }
                    """)
                },
                {
                    @"C:\corrupt.json",
                    new MockFileData("this is not json")
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

        Location[] expected = [
            new Location("SN123", "Array 1", 180),
            new Location("SN124", "Array 1", 180),
            new Location("SN125", "Array 2", 90)
        ];

        // Assert
        Assert.Equal(expected, locator.Locations);
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

