using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Caching.Distributed
{
    public interface IDistributedCache
    {
        byte[] Get(string key);
        Task<byte[]> GetAsync(string key, CancellationToken token = default);
        Task<string> GetStringAsync(string key);
        void Refresh(string key);
        Task RefreshAsync(string key, CancellationToken token = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken token = default);
        void Set(string key, byte[] value, DistributedCacheEntryOptions options);
        Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);
        Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options);
    }
}
