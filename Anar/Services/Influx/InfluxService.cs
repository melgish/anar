namespace Anar.Services.Influx;

using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;

/// <summary>
/// Accessor to write data to influxdb in batches of points.
/// </summary>
internal interface IInfluxService
{
    /// <summary>
    /// Writes a collection of point values to influx db
    /// </summary>
    /// <param name="points">Collection of points to write</param>
    /// <param name="cancellationToken"></param>
    Task WritePointsAsync(List<PointData> points, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implements IInfluxService
/// </summary>
internal sealed class InfluxService(
    IInfluxDbClientFactory clientFactory,
    IOptions<InfluxOptions> options,
    ILogger<InfluxService> logger
) : IInfluxService
{
    /// <summary>
    /// Writes a collection of point values to influx db
    /// </summary>
    /// <param name="points">Collection of points to write</param>
    /// <param name="cancellationToken"></param>
    public async Task WritePointsAsync(List<PointData> points, CancellationToken cancellationToken = default)
    {
        try
        {
            var opts = options.Value;

            using var client = clientFactory.CreateClient(opts.Uri, opts.Token);
            var api = client.GetWriteApiAsync();

            await api.WritePointsAsync(points, opts.Bucket, opts.Organization, cancellationToken);
            logger.LogDebug(LogEvents.WrotePoints, "Wrote {Count} points to {Bucket}", points.Count, opts.Bucket);
        }
        catch (Exception ex)
        {
            logger.LogWarning(LogEvents.InfluxDbWriteError, ex, "Failed to write points");
        }
    }
}