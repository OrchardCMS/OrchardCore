using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OrchardCore.DynamicCache.Services;

public class DefaultDynamicCache : IDynamicCache, IDisposable
{
    private readonly IDistributedCache _distributedCache;

    // When the distributed cache is the in-memory one, the values are kept in memory as they are given, so the
    // cached texts are not encoded when they are cached and decoded again each time they are rendered.
    private readonly MemoryCache _memoryCache;

    public DefaultDynamicCache(IDistributedCache distributedCache, IOptions<MemoryDistributedCacheOptions> memoryCacheOptions)
    {
        _distributedCache = distributedCache;

        if (distributedCache is MemoryDistributedCache)
        {
            _memoryCache = new MemoryCache(memoryCacheOptions.Value);
        }
    }

    public Task<byte[]> GetAsync(string key)
    {
        if (_memoryCache is null)
        {
            return _distributedCache.GetAsync(key);
        }

        return Task.FromResult(_memoryCache.Get<Entry>(key)?.Bytes);
    }

    public Task<string> GetStringAsync(string key)
    {
        if (_memoryCache is null)
        {
            var task = _distributedCache.GetAsync(key);

            if (task.IsCompletedSuccessfully)
            {
                return Task.FromResult(Decode(task.Result));
            }

            return Awaited(task);
        }

        return Task.FromResult(_memoryCache.Get<Entry>(key)?.Text);

        static async Task<string> Awaited(Task<byte[]> task) => Decode(await task);
    }

    public Task RemoveAsync(string key)
    {
        if (_memoryCache is null)
        {
            return _distributedCache.RemoveAsync(key);
        }

        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        if (_memoryCache is null)
        {
            return _distributedCache.SetAsync(key, value, options);
        }

        ArgumentNullException.ThrowIfNull(value);

        Set(key, new Entry(value, null), value.Length, options);

        return Task.CompletedTask;
    }

    public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (_memoryCache is null)
        {
            return _distributedCache.SetAsync(key, Encoding.UTF8.GetBytes(value), options);
        }

        Set(key, new Entry(null, value), Encoding.UTF8.GetByteCount(value), options);

        return Task.CompletedTask;
    }

    private void Set(string key, Entry entry, long size, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(options);

        // Same entry options as the ones used by the in-memory distributed cache.
        using var cacheEntry = _memoryCache.CreateEntry(key);
        cacheEntry.AbsoluteExpiration = options.AbsoluteExpiration;
        cacheEntry.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        cacheEntry.SlidingExpiration = options.SlidingExpiration;
        cacheEntry.Size = size;
        cacheEntry.Value = entry;
    }

    public void Dispose() => _memoryCache?.Dispose();

    private static string Decode(byte[] bytes) => bytes is null ? null : Encoding.UTF8.GetString(bytes);

    /// <summary>
    /// A value cached in memory as it was given, the other representation being created on demand.
    /// </summary>
    private sealed class Entry
    {
        private byte[] _bytes;
        private string _text;

        public Entry(byte[] bytes, string text)
        {
            _bytes = bytes;
            _text = text;
        }

        public byte[] Bytes => _bytes ??= Encoding.UTF8.GetBytes(_text);

        public string Text => _text ??= Encoding.UTF8.GetString(_bytes);
    }
}
