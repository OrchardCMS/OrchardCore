using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services;

/// <summary>
/// Wrapper preventing the <see cref="RedisCache"/> to dispose a shared <see cref="IConnectionMultiplexer"/>.
/// </summary>
public class RedisCacheWrapper : IDistributedCache
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
