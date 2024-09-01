namespace Anar.Services.Influx;

using InfluxDB.Client;

/// <summary>
/// Abstraction for creating InfluxDBClient instances.
/// </summary>
internal interface IInfluxDbClientFactory
{
    IInfluxDBClient CreateClient(Uri uri, string token);
}

/// <summary>
/// Factory for creating InfluxDBClient instances.
/// </summary>
internal sealed class InfluxDbClientFactory : IInfluxDbClientFactory
{
    public IInfluxDBClient CreateClient(Uri uri, string token)
    {
        return new InfluxDBClient(uri.ToString(), token);
    }
}