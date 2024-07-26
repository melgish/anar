using Anar.Services.Gateway;
using Anar.Services.Locator;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Anar.Services.Worker;

internal sealed record Reading(Inverter Inverter, Location? Location)
{
    /// <summary>
    /// Convert the supplied reading to a PointData instance
    /// </summary>
    /// <returns></returns>
    public PointData ToPointData() => PointData
        .Measurement("inverter")
        .Timestamp(Inverter.LastReportDate, WritePrecision.S)
        .Field("watts", Inverter.LastReportWatts)
        .Tag("serialNumber", Inverter.SerialNumber)
        .When(
            (_) => Location is not null,
            (p) => p.Tag("arrayName", Location!.ArrayName).Tag("facing", Location!.Facing)
        );
}

