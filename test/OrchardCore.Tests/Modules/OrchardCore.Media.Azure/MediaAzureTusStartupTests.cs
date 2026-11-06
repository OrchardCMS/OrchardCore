using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure;
using OrchardCore.Media.Azure.Services;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

public sealed class MediaAzureTusStartupTests
{
    private const string ConnectionString = "UseDevelopmentStorage=true";

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(ConnectionString, null)]
    [InlineData(null, "media")]
    public void ConfigureServices_MediaStoreNotActive_KeepsDefaultTusStore(string connectionString, string containerName)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITusTempStore>());

        // Act
        CreateStartup("Media:Azure", connectionString, containerName).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ImplementationType == typeof(AzureBlobTusTempStore));
    }

    [Theory]
    [InlineData("Media:Azure")]
    [InlineData("OrchardCore_Media_Azure")]
    public void ConfigureServices_MediaStoreActive_ReplacesTusStore(string section)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITusTempStore>());

        // Act
        CreateStartup(section, ConnectionString, "media").ConfigureServices(services);

        // Assert
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ITusTempStore));
        Assert.Equal(typeof(AzureBlobTusTempStore), descriptor.ImplementationType);
    }

    private static MediaAzureTusStartup CreateStartup(string section, string connectionString, string containerName)
        => new(new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{section}:ConnectionString"] = connectionString,
                [$"{section}:ContainerName"] = containerName,
            })
            .Build()));
}
