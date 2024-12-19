using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services;

/// <summary>
/// Wrapper preventing the <see cref="RedisCache"/> to dispose a shared <see cref="IConnectionMultiplexer"/>.
/// </summary>
/// <remarks>
/// This is done by not disposing the `RedisCache` instance which would otherwise close the redis connection.
/// </remarks>
internal sealed class RedisCacheWrapper : IDistributedCache
{
    private readonly IDistributedCache _cache;

    public RedisCacheWrapper(IDistributedCache cache) => _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    public byte[] Get(string key) => _cache.Get(key);

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _cache.GetAsync(key, token);

    public void Refresh(string key) => _cache.Refresh(key);

    public Task RefreshAsync(string key, CancellationToken token = default) => _cache.RefreshAsync(key, token);

    public void Remove(string key) => _cache.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default) => _cache.RemoveAsync(key, token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _cache.Set(key, value, options);

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        => _cache.SetAsync(key, value, options, token);
}
