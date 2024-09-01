// spell-checker: words Enphase
namespace Anar.Services.Gateway;

using System.Text.Json.Serialization;

/// <summary>
/// Represents inverter data from the Enphase gateway.
/// </summary>
internal sealed record Inverter
{
    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; init; } = string.Empty;

    [JsonPropertyName("lastReportDate")]
    public int LastReportDate { get; init; }

    [JsonPropertyName("lastReportWatts")]
    public int LastReportWatts { get; init; }
}