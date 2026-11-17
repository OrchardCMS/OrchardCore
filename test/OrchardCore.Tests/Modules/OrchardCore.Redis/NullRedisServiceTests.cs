using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Redis;
using OrchardCore.Redis.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public sealed class NullRedisServiceTests
{
    private static readonly ShellSettings s_shellSettings = new() { Name = "Default" };

    [Fact]
    public async Task ConnectAsync_Called_KeepsConnectionAndDatabaseNull()
    {
        // Arrange
        var redisService = new NullRedisService();

        // Act
        await redisService.ConnectAsync();

        // Assert
        Assert.Null(redisService.Connection);
        Assert.Null(redisService.Database);
    }

    [Fact]
    public async Task TryAcquireLockAsync_NullRedisService_DoesNotAcquireLock()
    {
        // Arrange
        var redisLock = new RedisLock(new NullRedisService(), s_shellSettings, NullLogger<RedisLock>.Instance);

        // Act
        var (locker, locked) = await redisLock.TryAcquireLockAsync("key", TimeSpan.FromMilliseconds(1));

        // Assert
        Assert.Null(locker);
        Assert.False(locked);
        Assert.False(await redisLock.IsLockAcquiredAsync("key"));
    }

    [Fact]
    public async Task PublishAsync_NullRedisService_DoesNotThrow()
    {
        // Arrange
        var redisBus = new RedisBus(new NullRedisService(), s_shellSettings, NullLogger<RedisBus>.Instance);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await redisBus.SubscribeAsync("channel", (channel, message) => { });
            await redisBus.PublishAsync("channel", "message");
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetTaggedItemsAsync_NullRedisService_ReturnsNoItems()
    {
        // Arrange
        var tagCache = new RedisTagCache(new NullRedisService(), s_shellSettings, [], NullLogger<RedisTagCache>.Instance);

        // Act
        await tagCache.TagAsync("key", "tag");
        var items = await tagCache.GetTaggedItemsAsync("tag");
        await tagCache.RemoveTagAsync("tag");

        // Assert
        Assert.Empty(items);
    }
}
