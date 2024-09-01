using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Text.Json;

namespace Anar.Services.Locator;

internal interface ILocatorService
{
    /// <summary>
    /// Gets list of known inverter locations.
    /// </summary>
    /// <param name="serialNumber">Serial Number of the inverter to find</param>
    /// <returns>Location collection, or empty collection</returns>
    IList<Location> Locations { get; }
}

/// <summary>
/// Implementation of ILocatorService that reads the array layout from a local
/// file.
/// </summary>
internal sealed class LocatorService(
    IFileSystem fileSystem,
    IOptions<LocatorOptions> options,
    ILogger<LocatorService> logger
) : ILocatorService
{
    /// <summary>
    /// Lazy-loaded list of locations.
    /// </summary>
    private IList<Location>? _locations;

    public IList<Location> Locations =>
        _locations ??= LoadFromFile(options.Value.LayoutFile);

    /// <summary>
    /// Reads the layout file and creates a dictionary of inverter locations.
    /// </summary>
    /// <param name="fileSystem">File system accessor.</param>
    /// <param name="options">Configuration options accessor.</param>
    /// <param name="logger">Logger instance.e</param>
    internal IList<Location> LoadFromFile(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                logger.LogInformation(LogEvents.NoLayoutFile, "No layout file specified");
                return [];
            }

            if (!fileSystem.File.Exists(fileName))
            {
                logger.LogError(LogEvents.LayoutFileNotFound, "Layout file {File} does not exist", fileName);
                return [];
            }

            var text = fileSystem.File.ReadAllText(fileName);
            var dto = JsonSerializer.Deserialize<LayoutDTO>(text);
            var list = dto?.ToLocations().ToList() ?? [];
            logger.LogInformation(LogEvents.LayoutFileLoaded, "Loaded {Count} locations from {File}", list.Count, fileName);
            return list;
        }
        catch (Exception ex)
        {
            logger.LogError(LogEvents.LayoutFileFormatError, ex, "Failed to parse {File}", fileName);
            return [];
        }
    }
}