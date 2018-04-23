using System.Threading.Tasks;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Services
{
    public interface IDynamicCacheService
    {
        Task<string> GetCachedValueAsync(CacheContext context);
        Task SetCachedValueAsync(CacheContext context, string value);
        Task<string> ReplaceEdgeSideIncludeTokensAsync(string value);
        Task<string> BuildEdgeSideIncludeTokenAsync(CacheContext context);
    }
}