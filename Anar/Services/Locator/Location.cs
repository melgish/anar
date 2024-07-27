namespace Anar.Services.Locator;

/// <summary>
/// Model for matching inverter data with array and facing information.
/// </summary>
/// <param name="SerialNumber">Identifies the inverter</param>
/// <param name="ArrayName">Name of array where inverter is located</param>
/// <param name="Azimuth">Azimuth of the array</param>
internal sealed record Location(string SerialNumber, string ArrayName, int Azimuth)
{
    /// <summary>
    /// Convert Azimuth into a facing value.
    /// </summary>
    public string Facing { get; private set; } = Azimuth switch
    {
        0 => "north",
        90 => "east",
        180 => "south",
        270 => "west",
        _ => "unknown"
    };
}