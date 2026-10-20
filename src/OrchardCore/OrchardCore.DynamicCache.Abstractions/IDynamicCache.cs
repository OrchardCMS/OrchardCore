using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.DynamicCache;

public interface IDynamicCache
{
    Task<byte[]> GetAsync(string key);
    Task RemoveAsync(string key);
    Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options);

    /// <summary>
    /// Gets a cached text. By default it is decoded from the cached bytes, implementations that keep
    /// the values in memory can return the text they were given by <see cref="SetStringAsync"/> directly.
    /// </summary>
    Task<string> GetStringAsync(string key)
    {
        var task = GetAsync(key);

        if (task.IsCompletedSuccessfully)
        {
            return Task.FromResult(Decode(task.Result));
        }

        return Awaited(task);

        static async Task<string> Awaited(Task<byte[]> task) => Decode(await task);

        static string Decode(byte[] bytes) => bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Caches a text. By default it is encoded to the cached bytes.
    /// </summary>
    Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options)
        => SetAsync(key, Encoding.UTF8.GetBytes(value), options);
}
