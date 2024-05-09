using Anar;
using Anar.Extensions;
using Anar.Services;

using Serilog;

// The down side to doing this here is that starup logging used base config.
Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateBootstrapLogger();

try {
  var builder = Host.CreateApplicationBuilder(args);
  builder.Configuration.AddDockerConfiguration();

  // Now that configuration is augmented add the real logger.
  builder.Services.AddSerilog((services, cfg) => cfg
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
  );

  builder.Services.AddGatewayClient();
  builder.Services.AddInfluxDB();
  builder.Services.AddHostedService<Worker>();

  var host = builder.Build();
  host.Run();
} catch (Exception ex) {
  Log.Fatal(ex, "Host terminated unexpectedly");
} finally {
  Log.CloseAndFlush();
}
