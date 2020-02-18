using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    /// <summary>
    /// Manages the cache for sitemap sources.
    /// </summary>
    public interface ISitemapTypeCacheManager
    {
        Task ClearCacheAsync(SitemapCacheContext context);
    }
}
