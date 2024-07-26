using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Anar.Services.Worker;

// https://enphase.com/download/accessing-iq-gateway-local-apis-or-local-ui-token-based-authentication
// Early on, it was possible to read totals from an endpoint.
// /api/v1/production. At some point my system firmware was upgraded and
// that endpoint only returns zeros.  The alternative endpoint
// ivp/pdm/energy does not work either. That one returns a 401.

internal sealed record Totals(int Watts = 0, int Timestamp = 0)
{
    public static Totals FromReadings(IEnumerable<Reading> readings) =>
        new Totals(
            readings.Sum(r => r.Inverter.LastReportWatts),
            readings.Max(r => r.Inverter.LastReportDate)
        );

    public PointData ToPointData() => PointData
        .Measurement("totals")
        .Field("wattsNow", Watts)
        .Timestamp(Timestamp, WritePrecision.S);
}
