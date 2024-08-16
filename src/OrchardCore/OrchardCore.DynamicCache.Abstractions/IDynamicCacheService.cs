using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache;

public interface IDynamicCacheService : ITagRemovedEventHandler
{
    Task<string> GetCachedValueAsync(CacheContext context);
    Task SetCachedValueAsync(CacheContext context, string value);
}
