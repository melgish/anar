namespace Anar.Services;

internal interface IInfluxService {
    /// <summary>
    /// Writes values for each inverter, as well as totals
    /// </summary>
    /// <param name="inverters">Collection of inverters to write</param>
    /// <param name="cancellationToken"></param>
    Task WriteInvertersAsync(IEnumerable<Inverter> inverters, CancellationToken cancellationToken = default);
}