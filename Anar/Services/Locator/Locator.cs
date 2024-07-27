using System.IO.Abstractions;
using System.Text.Json;

using Microsoft.Extensions.Options;

namespace Anar.Services.Locator;

internal interface ILocator
{
    /// <summary>
    /// Gets list of known inverter locations.
    /// </summary>
    /// <param name="serialNumber">Serial Number of the inverter to find</param>
    /// <returns>Location collection, or empty collection</returns>
    IList<Location> Locations { get; }
}

/// <summary>
/// Implementation of ILocator that reads the array layout from a local file.
/// </summary>
internal sealed class Locator(
    IFileSystem fileSystem,
    LocatorOptions options,
    ILogger<Locator> logger
) : ILocator
{
    /// <summary>
    /// Lazy-loaded list of locations.
    /// </summary>
    private IList<Location>? _locations;

    public IList<Location> Locations =>
        _locations ??= LoadFromFile(options.LayoutFile);

    /// <summary>
    /// Reads the layout file and creates a dictionary of inverter locations.
    /// </summary>
    /// <param name="options">Configuration options accessor.</param>
    /// <param name="logger">Logger instance.e</param>
    public Locator(IOptions<LocatorOptions> options, ILogger<Locator> logger)
        : this(new FileSystem(), options.Value, logger) { }

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
                logger.LogInformation("No layout file specified");
                return [];
            }

            if (!fileSystem.File.Exists(fileName))
            {
                logger.LogError("Layout file {File} does not exist", fileName);
                return [];
            }

            var text = fileSystem.File.ReadAllText(fileName);
            var dto = JsonSerializer.Deserialize<LayoutDTO>(text);
            var list = dto?.ToLocations().ToList() ?? [];
            logger.LogInformation("Loaded {Count} locations from {File}", list.Count, fileName);
            return list;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse {File}", fileName);
            return [];
        }
    }
}