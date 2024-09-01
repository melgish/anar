namespace Anar.Tests.Extensions;

using Anar.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System.Reflection;

public sealed class DockerExtensionsTests()
{
    [Fact]
    public void AddDockerConfiguration_WhenEnvironmentIsNotSet_DoesNotAdd()
    {
        Environment.SetEnvironmentVariable(DockerExtensions.APP_SETTINGS_DOCKER, null);
        var manager = new ConfigurationManager().AddDockerConfiguration();

        Assert.NotNull(manager);
        Assert.Empty(manager.Sources.OfType<JsonConfigurationSource>());
    }

    [Fact]
    public void AddDockerConfiguration_WhenFileDoesNotExist_DoesNotAdd()
    {
        Environment.SetEnvironmentVariable(DockerExtensions.APP_SETTINGS_DOCKER, @"A:\settings.json");
        var manager = new ConfigurationManager().AddDockerConfiguration();

        Assert.NotNull(manager);
        Assert.Empty(manager.Sources.OfType<JsonConfigurationSource>());
    }

    [Fact]
    public void AddDockerConfiguration_WhenFileDoesNotExist_Adds()
    {
        var dll = Assembly.GetExecutingAssembly().GetAssemblyLocation();
        var path = Path.Join(Path.GetDirectoryName(dll), "appsettings.json");

        Environment.SetEnvironmentVariable(DockerExtensions.APP_SETTINGS_DOCKER, path);
        var manager = new ConfigurationManager();
        // Intentionally adding same file twice to cover Select(e => e.Index + 1)
        manager.AddJsonFile(path, optional: true, reloadOnChange: false);
        manager.AddDockerConfiguration();

        Assert.NotNull(manager);
        Assert.Equal(2, manager.Sources.OfType<JsonConfigurationSource>().Count());
    }
}