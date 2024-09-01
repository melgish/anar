namespace Anar.Services.Locator;

using System.Text.Json.Serialization;

// An abridged version  of the data from array_layout_x.json downloaded from
// enlighten site is:
//  {
//    "arrays": [
//      {
//        "label": "Array 1",
//        "azimuth": 180,
//        "modules": [
//          {   "inverter": { "serial_num": "1" } }
//        ]
//      }
//    ]
//  }
// The classes below provide the means to parse the JSON data into C# objects.

internal sealed class LayoutDTO
{
    [JsonPropertyName("arrays")]
    public LayoutArray[] Arrays { get; init; } = [];

    public IEnumerable<Location> ToLocations()
        => Arrays.SelectMany(array => array.ToLocations());
}

internal sealed class LayoutArray
{
    [JsonPropertyName("label")]
    public string ArrayName { get; init; } = string.Empty;

    [JsonPropertyName("azimuth")]
    public int Azimuth { get; init; }

    [JsonPropertyName("modules")]
    public ArrayModule[] Modules { get; init; } = [];

    public IEnumerable<Location> ToLocations()
        => Modules.Select(mod => new Location(
            mod.Inverter.SerialNumber,
            ArrayName,
            Azimuth
        ));
}

internal sealed class ArrayModule
{
    [JsonPropertyName("inverter")]
    public ModuleInverter Inverter { get; init; } = new();
}

internal sealed class ModuleInverter
{
    [JsonPropertyName("serial_num")]
    public string SerialNumber { get; init; } = string.Empty;
}