using Microsoft.Extensions.Hosting;
using System.Reflection;

internal static class BackgroundServiceExtensions
{
    /// <summary>
    /// Invoke the ExecuteAsync method on a BackgroundService using reflection.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public static Task InvokeExecuteAsync(
        this BackgroundService service,
        CancellationToken stoppingToken
    )
    {
        return (Task)service
            .GetType()
            .GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(service, new object[] { stoppingToken });
    }
}