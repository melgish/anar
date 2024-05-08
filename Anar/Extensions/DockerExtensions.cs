using Microsoft.Extensions.Configuration.Json;

namespace Anar.Extensions;

public static class DockerExtensions {
  public const string APP_SETTINGS_DOCKER = "APP_SETTINGS_DOCKER";
  /// <summary>
  /// Does some fuckery to make sure that ENV and CommandLine are still the
  /// final arbiters of configuration values, while adding support for some
  /// additional locations tailored towards a Docker container.
  /// </summary>
  /// <param name="configuration">Configuration to manipulate</param>
  /// <returns></returns>
  public static ConfigurationManager AddDockerConfiguration(this ConfigurationManager configuration) {
    var fileName = Environment.GetEnvironmentVariable(APP_SETTINGS_DOCKER);
    if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
    {
      return configuration;
    }

    // default sources are:
    //
    // MemoryConfigurationSource (unknown)
    // MemoryConfigurationSource (contentRoot)
    // EnvironmentVariablesConfigurationSource (Prefix = DOTNET_)
    // CommandLineConfigurationSource (args)
    // JsonConfigurationSource ("appsettings.json")
    // JsonConfigurationSource ("appsettings.{Environment}.json")
    // JsonConfigurationSource ("secrets.json") [development only]
    // <<< new entry should go here when done >>>
    // EnvironmentVariablesConfigurationSource (Prefix = "")]
    // CommandLineConfigurationSource (args) [sometimes]
    var placement = configuration.Sources
        .Select((Source, Index) => new { Index, Source })
        .Where(e => e.Source is JsonConfigurationSource)
        .Select(e => e.Index + 1)
        .LastOrDefault(configuration.Sources.Count);

    // This will always add to the end. but there doesn't seem to be a well
    // documented way of creating one outside of 'AddJsonFile'. The typical
    // container path will be /configname or /run/secrets/secretname so the
    // added source needs to handle fully qualified paths.
    configuration.AddJsonFile(fileName, optional: true, reloadOnChange: false);

    // Pop the new one right back off and shuffle it to where it belongs.
    var source = configuration.Sources.Last()!;
    configuration.Sources.Remove(source);
    configuration.Sources.Insert(placement, source);
    return configuration;
  }
}