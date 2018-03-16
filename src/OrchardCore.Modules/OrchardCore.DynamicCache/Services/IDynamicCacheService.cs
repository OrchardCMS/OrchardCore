using System.Threading.Tasks;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    public interface IDynamicCacheService
    {
        Task<string> GetCachedValueAsync(string cacheId);
        Task SetCachedValueAsync(CacheContext context, string value); 
        Task<string> ProcessEdgeSideIncludesAsync(string value);
    }
}