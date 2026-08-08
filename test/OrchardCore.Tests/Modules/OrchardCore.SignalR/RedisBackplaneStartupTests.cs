using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Redis;
using OrchardCore.SignalR.Redis;
using SignalRRedisOptions = Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisOptions;
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
}
