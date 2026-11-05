using Microsoft.Azure.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Redis;
using OrchardCore.SignalR.Azure;
using AzureSignalRStartup = OrchardCore.SignalR.Azure.Startup;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using SignalRRedisStartup = OrchardCore.SignalR.Redis.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class AzureBackplaneStartupTests
{
    [Fact]
    public void ConfigureServices_WithoutConnectionString_DoesNotRegisterAzureSignalR()
    {
        // Arrange
        var services = new ServiceCollection();
        var startup = new AzureSignalRStartup(
            BuildShellConfiguration(connectionString: null),
            CreateShellSettings(),
            NullLogger<AzureSignalRStartup>.Instance);

        // Act
        startup.ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, IsAzureSignalRService);
    }

    [Fact]
    public void ConfigureServices_WithConnectionString_RegistersAzureSignalR()
    {
        // Arrange
        var services = new ServiceCollection();
        var startup = new AzureSignalRStartup(
            BuildShellConfiguration("Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;"),
            CreateShellSettings(),
            NullLogger<AzureSignalRStartup>.Instance);

        // Act
        startup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, IsAzureSignalRService);
    }

    [Fact]
    public void ConfigureServices_WithApplicationName_ConfiguresServiceOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["SignalR:Azure:ConnectionString"] = "Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;",
                ["SignalR:Azure:ApplicationName"] = "Application",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        var startup = new AzureSignalRStartup(
            new ShellConfiguration(configuration),
            CreateShellSettings(),
            NullLogger<AzureSignalRStartup>.Instance);

        // Act
        startup.ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ServiceOptions>>().Value;

        // Assert
        Assert.Equal("Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;", options.ConnectionString);
        Assert.Equal(AzureSignalRStartup.CreateApplicationName("Application", "Default"), options.ApplicationName);
    }

    [Fact]
    public void ConfigureServices_WithRedisBackplane_DoesNotRegisterAzureSignalRAndLogsError()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IRedisService>());

        new SignalRRedisStartup(NullLogger<SignalRRedisStartup>.Instance).ConfigureServices(services);

        var logger = new Mock<ILogger<AzureSignalRStartup>>();
        var startup = new AzureSignalRStartup(
            BuildShellConfiguration("Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;"),
            CreateShellSettings(),
            logger.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, IsAzureSignalRService);
        logger.Verify(l => l.Log(
            LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void TryBindConfiguration_WithoutConnectionString_FailsValidation(string connectionString)
    {
        // Arrange
        var options = new AzureSignalROptions();

        // Act
        var result = options.TryBindConfiguration(BuildShellConfiguration(connectionString));

        // Assert
        Assert.True(result);
        Assert.Single(options.Validate());
    }

    [Fact]
    public void TryBindConfiguration_WithConnectionString_BindsOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["SignalR:Azure:ConnectionString"] = "Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;",
                ["SignalR:Azure:ApplicationName"] = "Application",
            })
            .Build();

        var options = new AzureSignalROptions();

        // Act
        var result = options.TryBindConfiguration(new ShellConfiguration(configuration));

        // Assert
        Assert.True(result);
        Assert.Equal("Endpoint=https://tenant.service.signalr.net;AccessKey=abc123;Version=1.0;", options.ConnectionString);
        Assert.Equal("Application", options.ApplicationName);
        Assert.Empty(options.Validate());
    }

    [Fact]
    public void CreateApplicationName_DifferentTenants_UseDifferentValidNames()
    {
        // Act
        var alphaApplicationName = AzureSignalRStartup.CreateApplicationName(applicationName: null, "Alpha");
        var betaApplicationName = AzureSignalRStartup.CreateApplicationName(applicationName: null, "Beta");

        // Assert
        Assert.Matches("^[A-Za-z][A-Za-z0-9_]+$", alphaApplicationName);
        Assert.Matches("^[A-Za-z][A-Za-z0-9_]+$", betaApplicationName);
        Assert.NotEqual(alphaApplicationName, betaApplicationName);
    }

    [Fact]
    public void CreateApplicationName_DifferentApplications_UseDifferentNames()
    {
        // Act
        var firstApplicationName = AzureSignalRStartup.CreateApplicationName("FirstApplication", "Default");
        var secondApplicationName = AzureSignalRStartup.CreateApplicationName("SecondApplication", "Default");

        // Assert
        Assert.NotEqual(firstApplicationName, secondApplicationName);
    }

    private static bool IsAzureSignalRService(ServiceDescriptor descriptor)
        => descriptor.ServiceType.Namespace?.StartsWith("Microsoft.Azure.SignalR", StringComparison.Ordinal) == true;

    private static ShellSettings CreateShellSettings(string name = "Default")
        => new()
        {
            Name = name,
        };

    private static IShellConfiguration BuildShellConfiguration(string connectionString)
    {
        var settings = new Dictionary<string, string>();

        if (connectionString is not null)
        {
            settings["SignalR:Azure:ConnectionString"] = connectionString;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var shellConfiguration = new Mock<IShellConfiguration>();
        shellConfiguration
            .Setup(config => config.GetSection("SignalR:Azure"))
            .Returns(configuration.GetSection("SignalR:Azure"));

        return shellConfiguration.Object;
    }
}
