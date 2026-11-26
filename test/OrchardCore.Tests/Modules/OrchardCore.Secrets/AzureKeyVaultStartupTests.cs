using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking.Distributed;
using OrchardCore.Secrets;
using OrchardCore.Secrets.AzureKeyVault;
using KeyVaultStartup = OrchardCore.Secrets.AzureKeyVault.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

#pragma warning disable SCME0002 // Azure SDK configuration support is experimental.
public class AzureKeyVaultStartupTests
{
    public static TheoryData<string> InvalidNamePrefixes => [" ", new string('a', 257)];

    private static void ConfigureTenant(
        IServiceCollection services,
        string clientName = "SecretClient",
        string tenant = "Default",
        string namePrefix = null,
        ILogger<KeyVaultStartup> logger = null)
    {
        var configuration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Secrets:AzureKeyVault:AzureClient"] = clientName,
                ["Secrets:AzureKeyVault:NamePrefix"] = namePrefix,
            }));
        var shellSettings = new ShellSettings { Name = tenant };
        services.AddOptions();
        services.AddLogging();
        services.AddSingleton<IShellConfiguration>(configuration);
        services.AddSingleton(shellSettings);
        new KeyVaultStartup(shellSettings, configuration, logger ?? NullLogger<KeyVaultStartup>.Instance).ConfigureServices(services);
    }

    private static HostApplicationBuilder CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["AzureClients:SecretClient:VaultUri"] = "https://test.vault.azure.net/",
            ["AzureClients:SecretClient:Credential:CredentialSource"] = "ManagedIdentityCredential",
            ["AzureClients:SecretClient:Credential:ManagedIdentityIdKind"] = "SystemAssigned",
            ["AzureClients:SecretClient:Options:Retry:MaxRetries"] = "3",
            ["AzureClients:SecretClient:Options:Retry:Delay"] = "00:00:00.800",
            ["AzureClients:SecretClient:Options:Retry:MaxDelay"] = "00:01:00",
            ["AzureClients:SecretClient:Options:Retry:Mode"] = "Exponential",
            ["AzureClients:SecretClient:Options:Retry:NetworkTimeout"] = "00:01:40",
            ["AzureClients:SecretClient:Options:Diagnostics:ApplicationId"] = "my-app",
            ["AzureClients:SecretClient:Options:Diagnostics:IsLoggingEnabled"] = "true",
            ["AzureClients:SecretClient:Options:Diagnostics:IsTelemetryEnabled"] = "true",
            ["AzureClients:SecretClient:Options:Diagnostics:IsDistributedTracingEnabled"] = "true",
            ["AzureClients:SecretClient:Options:Diagnostics:IsLoggingContentEnabled"] = "false",
            ["AzureClients:SecretClient:Options:Diagnostics:LoggedContentSizeLimit"] = "4096",
            ["AzureClients:SecretClient:Options:Diagnostics:AdditionalLoggedHeaderNames:0"] = "x-orchard-request-id",
            ["AzureClients:SecretClient:Options:Diagnostics:AdditionalLoggedQueryParameters:0"] = "orchard-request-id",
            ["AzureClients:SecretClient:Options:DisableChallengeResourceVerification"] = "false",
        });
        builder.AddKeyedAzureClient<SecretClient, SecretClientSettings>("SecretClient", "AzureClients:SecretClient");
        return builder;
    }

    [Fact]
    public void Startup_ResolvesSdkRegisteredNamedClient()
    {
        var builder = CreateHost();
        ConfigureTenant(builder.Services);
        using var provider = builder.Services.BuildServiceProvider();

        var client = provider.GetRequiredKeyedService<SecretClient>("SecretClient");

        Assert.Equal(new Uri("https://test.vault.azure.net/"), client.VaultUri);
        Assert.Same(client, provider.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        Assert.Null(provider.GetService<SecretClient>());
        Assert.IsType<AzureKeyVaultSecretStore>(Assert.Single(provider.GetServices<ISecretStore>()));
    }

    [Fact]
    public void Startup_TenantContainersShareHostClient()
    {
        var builder = CreateHost();
        using var host = builder.Build();
        var firstServices = host.Services.CreateChildContainer(builder.Services);
        var secondServices = host.Services.CreateChildContainer(builder.Services);
        ConfigureTenant(firstServices, tenant: "First");
        ConfigureTenant(secondServices, tenant: "Second");
        using var first = firstServices.BuildServiceProvider();
        using var second = secondServices.BuildServiceProvider();

        var client = host.Services.GetRequiredKeyedService<SecretClient>("SecretClient");

        Assert.Same(client, first.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        Assert.Same(client, second.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        Assert.NotSame(first.GetRequiredService<ISecretStore>(), second.GetRequiredService<ISecretStore>());
    }

    [Fact]
    public async Task Startup_LazilyResolvesSelectedClientWithoutReplacingOtherFeaturesClients()
    {
        var selected = new Mock<SecretClient>();
        selected.Setup(client => client.GetSecretAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        var other = new Mock<SecretClient>();
        var resolutions = 0;
        var services = new ServiceCollection();
        services.AddKeyedSingleton<SecretClient>("SharedVault", (_, _) =>
        {
            resolutions++;
            return selected.Object;
        });
        services.AddKeyedSingleton("OtherFeature", other.Object);
        services.AddSingleton(other.Object);
        ConfigureTenant(services, "SharedVault");
        using var provider = services.BuildServiceProvider();

        Assert.Equal(0, resolutions);
        Assert.Null(await provider.GetRequiredService<ISecretStore>().GetSecretAsync<TextSecret>("Missing"));
        Assert.Same(selected.Object, provider.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        Assert.Equal(1, resolutions);
        Assert.Same(other.Object, provider.GetRequiredKeyedService<SecretClient>("OtherFeature"));
        Assert.Same(other.Object, provider.GetRequiredService<SecretClient>());
        selected.Verify(client => client.GetSecretAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
        other.Verify(client => client.GetSecretAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Settings_SdkBindsGlobalClientAndCredentialOptions()
    {
        var builder = CreateHost();

        var settings = builder.Configuration.GetAzureClientSettings<SecretClientSettings>("AzureClients:SecretClient");

        Assert.Equal("ManagedIdentityCredential", settings.Credential.CredentialSource, ignoreCase: true);
        Assert.IsType<ManagedIdentityCredential>(settings.CredentialProvider);
        Assert.False(settings.Options.DisableChallengeResourceVerification);
        Assert.Equal(3, settings.Options.Retry.MaxRetries);
        Assert.Equal(RetryMode.Exponential, settings.Options.Retry.Mode);
        Assert.Equal(TimeSpan.FromMilliseconds(800), settings.Options.Retry.Delay);
        Assert.Equal(TimeSpan.FromMinutes(1), settings.Options.Retry.MaxDelay);
        Assert.Equal(TimeSpan.FromSeconds(100), settings.Options.Retry.NetworkTimeout);
        Assert.Equal("my-app", settings.Options.Diagnostics.ApplicationId);
        Assert.True(settings.Options.Diagnostics.IsLoggingEnabled);
        Assert.True(settings.Options.Diagnostics.IsTelemetryEnabled);
        Assert.True(settings.Options.Diagnostics.IsDistributedTracingEnabled);
        Assert.False(settings.Options.Diagnostics.IsLoggingContentEnabled);
        Assert.Equal(4096, settings.Options.Diagnostics.LoggedContentSizeLimit);
        Assert.Contains("x-orchard-request-id", settings.Options.Diagnostics.LoggedHeaderNames);
        Assert.Contains("orchard-request-id", settings.Options.Diagnostics.LoggedQueryParameters);
    }

    [Fact]
    public void Startup_TenantsCanSelectDifferentHostClients()
    {
        var builder = CreateHost();
        var other = Mock.Of<SecretClient>();
        builder.Services.AddKeyedSingleton("OtherVault", other);
        using var host = builder.Build();
        var firstServices = host.Services.CreateChildContainer(builder.Services);
        var secondServices = host.Services.CreateChildContainer(builder.Services);
        ConfigureTenant(firstServices, tenant: "First");
        ConfigureTenant(secondServices, "OtherVault", "Second");
        using var first = firstServices.BuildServiceProvider();
        using var second = secondServices.BuildServiceProvider();

        Assert.Same(host.Services.GetRequiredKeyedService<SecretClient>("SecretClient"),
            first.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        Assert.Same(other, second.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
    }

    [Fact]
    public void Startup_SdkRegistrationResolvesSharedCredentialReference()
    {
        var builder = CreateHost();
        builder.Configuration["AzureClients:SecretClient:Credential:CredentialSource"] = null;
        builder.Configuration["AzureClients:SecretClient:Credential"] = "$Shared:Credential";
        builder.Configuration["Shared:Credential:CredentialSource"] = "AzureCliCredential";
        ConfigureTenant(builder.Services);
        using var provider = builder.Services.BuildServiceProvider();

        Assert.Equal(new Uri("https://test.vault.azure.net/"),
            provider.GetRequiredKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey).VaultUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(AzureKeyVaultSecretStore.SecretClientServiceKey)]
    public void ConfigureServices_InvalidClientName_DoesNotRegisterStore(string name)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSingleton("SecretClient", Mock.Of<SecretClient>());
        var logger = new ListLogger();

        // Act
        ConfigureTenant(services, name, logger: logger);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.Empty(provider.GetServices<ISecretStore>());
        Assert.Null(provider.GetKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("'Secrets:AzureKeyVault:AzureClient'", entry.Message);
    }

    [Fact]
    public void ConfigureServices_UnregisteredClient_DoesNotRegisterStoreOrFallBackToDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<SecretClient>());
        services.AddKeyedSingleton("OtherClient", Mock.Of<SecretClient>());
        var logger = new ListLogger();

        // Act
        ConfigureTenant(services, "MissingClient", logger: logger);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.Empty(provider.GetServices<ISecretStore>());
        Assert.Null(provider.GetKeyedService<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey));
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("'MissingClient'", entry.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidNamePrefixes))]
    public void ConfigureServices_InvalidNamePrefix_DoesNotRegisterStore(string namePrefix)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSingleton("SecretClient", Mock.Of<SecretClient>());
        var logger = new ListLogger();

        // Act
        ConfigureTenant(services, namePrefix: namePrefix, logger: logger);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.Empty(provider.GetServices<ISecretStore>());
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("'Secrets:AzureKeyVault:NamePrefix'", entry.Message);
    }

    [Fact]
    public void ConfigureServices_ValidConfiguration_LogsNothing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSingleton("SecretClient", Mock.Of<SecretClient>());
        var logger = new ListLogger();

        // Act
        ConfigureTenant(services, logger: logger);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsType<AzureKeyVaultSecretStore>(Assert.Single(provider.GetServices<ISecretStore>()));
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task GetSecretInfosAsync_MissingConfiguration_SecretManagerHasNoKeyVaultStore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSecrets();
        services.AddSingleton(Mock.Of<IDistributedLock>());
        ConfigureTenant(services, clientName: null);
        using var provider = services.BuildServiceProvider();

        // Act
        var manager = provider.GetRequiredService<ISecretManager>();
        var secrets = await manager.GetSecretInfosAsync();

        // Assert
        Assert.Empty(manager.GetStores());
        Assert.Empty(secrets);
    }

    private sealed class ListLogger : ILogger<KeyVaultStartup>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
#pragma warning restore SCME0002
