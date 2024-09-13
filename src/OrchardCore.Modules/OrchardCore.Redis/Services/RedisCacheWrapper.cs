using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services;

/// <summary>
/// Wrapper preventing the <see cref="RedisCache"/> to dispose a shared <see cref="IConnectionMultiplexer"/>.
/// </summary>
/// <remarks>
/// This class shouldn't implement <see cref="IDisposable"/> because it prevents the internal Redis cache service from being disposed of when
/// a tenant is restarted, or the <see cref="IConnectionMultiplexer"/> that is shared across tenants would be disposed too.
/// </remarks>
public sealed class RedisCacheWrapper : IDistributedCache
{
    private readonly RedisCache _cache;

    public RedisCacheWrapper(IOptions<RedisCacheOptions> optionsAccessor) => _cache = new RedisCache(optionsAccessor);

    public byte[] Get(string key) => _cache.Get(key);

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _cache.GetAsync(key, token);

    public void Refresh(string key) => _cache?.Refresh(key);

    public Task RefreshAsync(string key, CancellationToken token = default) => _cache.RefreshAsync(key, token);

    public void Remove(string key) => _cache!.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default) => _cache.RemoveAsync(key, token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _cache.Set(key, value, options);

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        => _cache.SetAsync(key, value, options, token);
}
