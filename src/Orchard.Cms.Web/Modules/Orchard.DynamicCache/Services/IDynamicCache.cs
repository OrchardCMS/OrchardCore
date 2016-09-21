using Microsoft.Extensions.Caching.Distributed;
using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.DynamicCache.Services
{
    public interface IDynamicCache : ISingletonDependency
    {
        Task<byte[]> GetAsync(string key);
        Task RemoveAsync(string key);
        Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options);
    }
}
