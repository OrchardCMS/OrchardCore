using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using OrchardCore.Caching.Distributed;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Redis;
using OrchardCore.Redis.Options;
using OrchardCore.Redis.Services;
using RedisStartup = OrchardCore.Redis.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public sealed class RedisStartupTests
{
    private const string ValidConfiguration = "localhost:6379,abortConnect=false";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknownKeyword=value")]
    public void ConfigureServices_MissingOrInvalidConfiguration_RegistersNullRedisService(string configuration)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        CreateStartup("Redis", configuration).ConfigureServices(services);

        // Assert
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IRedisService));
        Assert.Equal(typeof(NullRedisService), descriptor.ImplementationType);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRedisDatabaseFactory));
    }

    [Theory]
    [InlineData("Redis")]
    [InlineData("OrchardCore_Redis")]
    public void ConfigureServices_ValidConfiguration_RegistersRedisService(string section)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        CreateStartup(section, ValidConfiguration).ConfigureServices(services);

        // Assert
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IRedisService));
        Assert.Equal(typeof(RedisService), descriptor.ImplementationType);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IModularTenantEvents));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRedisDatabaseFactory));
    }

    [Fact]
    public void ConfigureServices_MissingConfigurationWithCustomRedisService_KeepsCustomRedisService()
    {
        // Arrange
        var redisService = Mock.Of<IRedisService>();
        var services = new ServiceCollection();
        services.AddSingleton(redisService);

        // Act
        CreateStartup("Redis", configuration: null).ConfigureServices(services);

        // Assert
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IRedisService));
        Assert.Same(redisService, descriptor.ImplementationInstance);
    }

    [Fact]
    public void ConfigureServices_ValidConfigurationAfterHealthCheck_RegistersRedisService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHealthChecks().AddRedisCheck();

        // Act
        CreateStartup("Redis", ValidConfiguration).ConfigureServices(services);

        // Assert
        Assert.True(services.IsRedisConfigured());
        Assert.Equal(typeof(RedisService), services.Last(descriptor => descriptor.ServiceType == typeof(IRedisService)).ImplementationType);
    }

    [Fact]
    public void GetRequiredService_MissingConfiguration_ActivatesServiceDependingOnRedisService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<RedisConsumer>();

        CreateStartup("Redis", configuration: null).ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var consumer = serviceProvider.GetRequiredService<RedisConsumer>();

        // Assert
        Assert.IsType<NullRedisService>(consumer.Redis);
    }

    [Fact]
    public void ConfigureServices_MissingConfiguration_FeaturesKeepDefaultImplementations()
    {
        // Act
        var services = ConfigureFeatures(configuration: null);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IDistributedCache));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IConfigureOptions<RedisCacheOptions>));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(ITagCache));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMessageBus));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IDistributedLock));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IConfigureOptions<KeyManagementOptions>));
    }

    [Fact]
    public void ConfigureServices_ValidConfiguration_FeaturesRegisterRedisImplementations()
    {
        // Act
        var services = ConfigureFeatures(ValidConfiguration);

        // Assert
        Assert.Contains(services, descriptor => IsRegistered<IDistributedCache, RedisCacheWrapper>(descriptor));
        Assert.Contains(services, descriptor => IsRegistered<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>(descriptor));
        Assert.Contains(services, descriptor => IsRegistered<ITagCache, RedisTagCache>(descriptor));
        Assert.Contains(services, descriptor => IsRegistered<IMessageBus, RedisBus>(descriptor));
        Assert.Contains(services, descriptor => IsRegistered<IDistributedLock, RedisLock>(descriptor));
        Assert.Contains(services, descriptor => IsRegistered<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>(descriptor));
    }

    private static ServiceCollection ConfigureFeatures(string configuration)
    {
        var services = new ServiceCollection();

        CreateStartup("Redis", configuration).ConfigureServices(services);
        new RedisCacheStartup().ConfigureServices(services);
        new RedisBusStartup().ConfigureServices(services);
        new RedisLockStartup().ConfigureServices(services);
        new RedisDataProtectionStartup().ConfigureServices(services);

        return services;
    }

    private static RedisStartup CreateStartup(string section, string configuration)
    {
        var shellConfiguration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{section}:Configuration"] = configuration,
            })
            .Build());

        return new RedisStartup(new ShellSettings { Name = "Default" }, shellConfiguration, NullLogger<RedisStartup>.Instance);
    }

    private static bool IsRegistered<TService, TImplementation>(ServiceDescriptor descriptor)
        => descriptor.ServiceType == typeof(TService) && descriptor.GetImplementationType() == typeof(TImplementation);

    private sealed class RedisConsumer
    {
        public RedisConsumer(IRedisService redis)
        {
            Redis = redis;
        }

        public IRedisService Redis { get; }
    }
}
