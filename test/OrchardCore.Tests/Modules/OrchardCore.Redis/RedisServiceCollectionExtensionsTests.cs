using OrchardCore.Redis;
using OrchardCore.Redis.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public sealed class RedisServiceCollectionExtensionsTests
{
    [Fact]
    public void IsRedisConfigured_WithoutRedisService_ReturnsFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.False(services.IsRedisConfigured());
    }

    [Fact]
    public void IsRedisConfigured_WithNullRedisService_ReturnsFalse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IRedisService, NullRedisService>();

        // Act & Assert
        Assert.False(services.IsRedisConfigured());
    }

    [Fact]
    public void IsRedisConfigured_WithRedisService_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IRedisService, RedisService>();

        // Act & Assert
        Assert.True(services.IsRedisConfigured());
    }

    [Fact]
    public void IsRedisConfigured_WithCustomInstance_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IRedisService>());

        // Act & Assert
        Assert.True(services.IsRedisConfigured());
    }

    [Fact]
    public void IsRedisConfigured_WithCustomFactory_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IRedisService>(serviceProvider => Mock.Of<IRedisService>());

        // Act & Assert
        Assert.True(services.IsRedisConfigured());
    }
}
