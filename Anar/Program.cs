using Anar;
using Anar.Extensions;
using Anar.Services;

var builder = Host.CreateApplicationBuilder(args);
// Wedge in some docker love.
builder.Configuration.AddDockerConfiguration();

builder.Services.AddGatewayClient();
builder.Services.AddInfluxDB();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
