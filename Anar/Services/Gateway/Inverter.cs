using System.Text.Json.Serialization;

namespace Anar.Services;

internal sealed class Inverter {
    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = default!;

    [JsonPropertyName("lastReportDate")]
    public int LastReportDate { get; set; }

    [JsonPropertyName("devType")]
    public int DeviceType { get; set; }

    [JsonPropertyName("lastReportWatts")]
    public int LastReportWatts { get; set; }

    [JsonPropertyName("maxReportWatts")]
    public int MaxReportWatts { get; set; }

    /// <summary>
    /// Additional information if available from array_layout_x.json
    /// </summary>
    [JsonIgnore]
    public Location? Location { get; set; }
}