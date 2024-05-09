namespace Anar.Services;

/// <summary>
/// Encapsulates the location of a single panel / inverter.
/// </summary>
public sealed class Location
{
    /// <summary>
    /// Name of the array where the inverter is located.
    /// </summary>
    public string ArrayName { get; init; } = default!;

    /// <summary>
    /// Direction the array (and inverter) is facing.
    /// </summary>
    public int Azimuth { get; init; }

    /// <summary>
    /// Serial number of the inverter.
    /// </summary>
    public string SerialNumber { get; init; } = default!;

    /// <summary>
    /// Convert facing to a string.
    /// </summary>
    public string Facing
    {
        get
        {
            switch (Azimuth)
            {
                case 0: return "north";
                case 90: return "east";
                case 180: return "south";
                case 270: return "west";
                default: return "unknown";
            }
        }
    }
}