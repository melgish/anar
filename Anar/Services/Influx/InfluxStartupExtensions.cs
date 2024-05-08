namespace Anar.Services;

internal static class InfluxDBStartupExtensions {
    public static void AddInfluxDB(this IServiceCollection services) {
        services
           .AddOptions<InfluxOptions>()
           .Configure<IConfiguration>((options, configuration) => {
                configuration.GetSection(nameof(InfluxOptions)).Bind(options);
            })
           .ValidateDataAnnotations()
           .ValidateOnStart();

        services.AddSingleton<IInfluxService, InfluxService>();
    }
}