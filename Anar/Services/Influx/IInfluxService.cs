using InfluxDB.Client.Writes;

namespace Anar.Services;

internal interface IInfluxService {
    Task WriteAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default);
}