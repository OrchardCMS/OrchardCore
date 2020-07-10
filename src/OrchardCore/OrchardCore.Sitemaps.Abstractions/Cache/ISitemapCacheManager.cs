using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    /// <summary>
    /// Manages the cache for sitemaps.
    /// </summary>
    public interface ISitemapCacheManager
    {
        Task ClearCacheAsync(SitemapCacheContext context);
    }
}
