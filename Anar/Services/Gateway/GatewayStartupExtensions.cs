using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Net.Security;
using System.Text.Json.Nodes;
using Serilog;

namespace Anar.Services;

internal static class GatewayStartupExtensions
{
    /// <summary>
    /// Loads position information about the system.
    /// </summary>
    /// <param name="layoutFile">layout file to parse</param>
    /// <returns>Array of locations, or empty array if not found</returns>
    /// <remarks>
    /// This info is not available via API, or directly from the gateway.
    ///
    /// Currently the only way to get it is to login to enlighten at
    /// https://enlighten.enphaseenergy.com and use developer tools to
    /// capture the response to the array_layout_x.json request.
    /// </remarks>
    private static Location[] GetArrayLayout(string layoutFile)
    {
#nullable disable
        try
        {
            var json = JsonNode.Parse(File.ReadAllText(layoutFile));
            return json["arrays"]
                .AsArray()
                .SelectMany(
                    a => a["modules"].AsArray(),
                    (a, m) => new Location
                    {
                        ArrayName = a["label"].GetValue<string>(),
                        Azimuth = a["azimuth"].GetValue<int>(),
                        SerialNumber = m["inverter"]["serial_num"].GetValue<string>(),
                    })
                    .ToArray();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to parse load array layout file.");
            return [];
        }
#nullable restore
    }

    public static void AddGatewayClient(this IServiceCollection services)
    {
        services
            .AddOptions<GatewayOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.Bind(nameof(GatewayOptions), options);

                // Allow layout to be specified entirely in the config file,
                // but if file is specified, import that instead.
                if (string.IsNullOrWhiteSpace(options.LayoutFile)) {
                    Log.Information("Layout file has not been provided");
                    return;
                }

                if (!File.Exists(options.LayoutFile)) {
                    Log.Warning("Layout file '{FileName}' not found", options.LayoutFile);
                    return;
                }

                options.Layout = GetArrayLayout(options.LayoutFile);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<IGatewayClient, GatewayClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GatewayOptions>>().Value;
                client.BaseAddress = options.Uri;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
            })
            .ConfigurePrimaryHttpMessageHandler((sp) =>
            {
                var options = sp.GetRequiredService<IOptions<GatewayOptions>>().Value;
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        // Certificate is already valid.
                        // Certificate matches supplied thumbprint.
                        return (errors == SslPolicyErrors.None)
                            || (options.Thumbprint == cert?.Thumbprint);
                    }
                };
            });
    }
}