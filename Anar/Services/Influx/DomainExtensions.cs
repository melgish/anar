using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Anar.Services;

internal static class DomainExtensions
{
    public static PointData ToPointData(this Inverter inverter)
    {
        var point = PointData
            .Measurement("inverter")
            .Timestamp(inverter.LastReportDate, WritePrecision.S)
            .Field("watts", inverter.LastReportWatts)
            .Tag("serialNumber", inverter.SerialNumber);

        if (inverter.Location is not null)
        {
            point = point
                .Tag("arrayName", inverter.Location.ArrayName)
                .Tag("facing", inverter.Location.Facing);
        }

        return point;
    }
}
