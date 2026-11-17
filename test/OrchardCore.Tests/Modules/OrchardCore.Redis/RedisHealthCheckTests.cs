using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Redis;
using OrchardCore.Redis.HealthChecks;
using StackExchange.Redis;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public sealed class RedisHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_NullRedisService_ReturnsUnhealthy()
    {
        // Arrange
        var healthCheck = new RedisHealthCheck(new NullRedisService());

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("'Configuration'", result.Description);
        Assert.Null(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_ConnectionFails_ReturnsUnhealthy()
    {
        // Arrange
        var redisService = new Mock<IRedisService>();
        var healthCheck = new RedisHealthCheck(redisService.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
        redisService.Verify(service => service.ConnectAsync(), Times.Once());
    }

    [Fact]
    public async Task CheckHealthAsync_NotConnected_ReturnsUnhealthy()
    {
        // Arrange
        var healthCheck = new RedisHealthCheck(CreateRedisService(isConnected: false));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Couldn't connect to the Redis server.", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_Connected_ReturnsHealthy()
    {
        // Arrange
        var healthCheck = new RedisHealthCheck(CreateRedisService(isConnected: true));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ResolvedFromServices_InjectsRedisService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRedisService, NullRedisService>();
        services.AddHealthChecks().AddRedisCheck();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var report = await serviceProvider.GetRequiredService<HealthCheckService>().CheckHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = Assert.Single(report.Entries).Value;
        Assert.Equal(HealthStatus.Unhealthy, entry.Status);
        Assert.Contains("'Configuration'", entry.Description);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_WithoutRedisFeature_ReturnsUnhealthy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks().AddRedisCheck();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var report = await serviceProvider.GetRequiredService<HealthCheckService>().CheckHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = Assert.Single(report.Entries).Value;
        Assert.Equal(HealthStatus.Unhealthy, entry.Status);
    }

    private static IRedisService CreateRedisService(bool isConnected)
    {
        var connection = new Mock<IConnectionMultiplexer>();
        connection
            .Setup(multiplexer => multiplexer.IsConnected)
            .Returns(isConnected);

        var database = new Mock<IDatabase>();
        database
            .Setup(redisDatabase => redisDatabase.PingAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMilliseconds(1));

        var redisService = new Mock<IRedisService>();
        redisService
            .Setup(service => service.Connection)
            .Returns(connection.Object);
        redisService
            .Setup(service => service.Database)
            .Returns(database.Object);

        return redisService.Object;
    }
}
