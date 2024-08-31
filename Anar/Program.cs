using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;

using Anar;
using Anar.Extensions;
using Anar.Services;
using Anar.Services.Gateway;
using Anar.Services.Influx;
using Anar.Services.Locator;
using Anar.Services.Worker;

using Microsoft.Extensions.Options;

using Serilog;

// Use static log during startup to log any configuration warnings or errors.
Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateBootstrapLogger();

try
{
    var name = Assembly.GetExecutingAssembly().GetName();
    Log.Information("{AssemblyName} v{Version}", name.Name, name.Version);

    var builder = Host.CreateApplicationBuilder(args);
    builder.Configuration.AddDockerConfiguration();

    // Now that configuration is augmented add the real logger
    // using appsettings and services.
    builder.Services.AddSerilog((services, cfg) => cfg
      .ReadFrom.Configuration(builder.Configuration)
      .ReadFrom.Services(services)
    );

    // Gateway
    builder.Services
        .AddSingleton<GatewayThumbprintValidator>()
        .AddSingleton<IGatewayService, GatewayService>()
        .AddHttpClient(nameof(GatewayService), (sp, client) => {
            var options = sp.GetRequiredService<IOptions<GatewayOptions>>().Value;
            client.BaseAddress = new Uri(options.Uri, options.RequestPath);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Application.Json));
            client.DefaultRequestHeaders.Authorization = new("Bearer", options.Token);
        })
        .ConfigurePrimaryHttpMessageHandler(sp => {
            var validator = sp.GetRequiredService<GatewayThumbprintValidator>();
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = validator.ValidateThumbprint
            };
        });

    builder.Services
        .AddOptions<GatewayOptions>()
        .BindConfiguration("Gateway")
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Influx
    builder.Services
        .AddSingleton<IInfluxService, InfluxService>()
        .AddOptions<InfluxOptions>()
        .Bind(builder.Configuration.GetSection("Influx"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Locator
    builder.Services
      .AddSingleton<ILocatorService, LocatorService>()
      .AddOptions<LocatorOptions>()
      .Bind(builder.Configuration.GetSection("Locator"))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    // Worker
    builder.Services
        .AddHostedService<WorkerService>()
        .AddOptions<WorkerOptions>()
        .Bind(builder.Configuration.GetSection("Worker"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}