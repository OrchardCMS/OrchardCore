using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Redis;
using OrchardCore.SignalR.Redis;
using SignalRRedisOptions = Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisOptions;
using AzureSignalRStartup = OrchardCore.SignalR.Azure.Startup;
using SignalRRedisStartup = OrchardCore.SignalR.Redis.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class RedisBackplaneStartupTests
{
    [Fact]
    public void ConfigureServices_WithoutRedisService_DoesNotRegisterBackplaneConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var startup = new SignalRRedisStartup();

        // Act
        startup.ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IConfigureOptions<SignalRRedisOptions>)
                && descriptor.ImplementationType == typeof(SignalRRedisOptionsConfiguration));
    }

    [Fact]
    public void ConfigureServices_WithRedisService_RegistersTenantQualifiedBackplaneConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IRedisService>());

        var startup = new SignalRRedisStartup();

        // Act
        startup.ConfigureServices(services);

        // Assert
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IConfigureOptions<SignalRRedisOptions>)
                && descriptor.ImplementationType == typeof(SignalRRedisOptionsConfiguration));
    }

    [Fact]
    public void ConfigureServices_WithAzureBackplane_ThrowsConfigurationError()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["SignalR:Azure:ConnectionString"] =
                    "Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;",
            })
            .Build();

        var shellConfiguration = new Mock<IShellConfiguration>();
        shellConfiguration
            .Setup(config => config.GetSection("SignalR:Azure"))
            .Returns(configuration.GetSection("SignalR:Azure"));

        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IRedisService>());

        new AzureSignalRStartup(
            shellConfiguration.Object,
            new ShellSettings { Name = "Default" },
            NullLogger<AzureSignalRStartup>.Instance)
            .ConfigureServices(services);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => new SignalRRedisStartup().ConfigureServices(services));

        // Assert
        Assert.Contains("cannot be enabled together", exception.Message);
    }
}
