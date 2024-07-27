using System.Text.Json.Serialization;

namespace Anar.Services.Gateway;

internal sealed record Inverter
{
    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; init; } = string.Empty;

    [JsonPropertyName("lastReportDate")]
    public int LastReportDate { get; init; }

    [JsonPropertyName("lastReportWatts")]
    public int LastReportWatts { get; init; }
}