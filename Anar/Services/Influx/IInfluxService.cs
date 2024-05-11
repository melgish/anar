using InfluxDB.Client.Writes;

namespace Anar.Services;

internal interface IInfluxService {
    /// <summary>
    /// Sums lastReportWatts for all inverters in the collection and writes
    /// total based on lastReportDate of the most recent inverter report..
    /// </summary>
    /// <param name="inverters">Collection of inverters to total</param>
    /// <param name="cancellationToken"></param>
    Task WriteTotalsAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes values for each inverter
    /// </summary>
    /// <param name="inverters">Collection of inverters to write</param>
    /// <param name="cancellationToken"></param>
    Task WriteInvertersAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default);
}