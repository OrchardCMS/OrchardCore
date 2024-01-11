using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.DynamicCache
{
    public interface IDynamicCache
    {
        Task<byte[]> GetAsync(string key);
        Task RemoveAsync(string key);
        Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options);
    }
}
