// spell-checker: words Enphase
namespace Anar.Services;

internal interface IGatewayClient
{
    /// <summary>
    /// Determines how often to poll inverters.
    /// </summary>
    TimeSpan Interval { get; }

    /// <summary>
    /// Requests inverter data from the Enphase gateway.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// List of inverter info when successful, otherwise an empty list</returns>
    Task<IEnumerable<Inverter>> GetInvertersAsync(CancellationToken cancellationToken = default);
}