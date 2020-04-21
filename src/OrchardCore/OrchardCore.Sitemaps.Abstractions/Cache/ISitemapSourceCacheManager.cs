using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    /// <summary>
    /// Manages the sitemap source item cache.
    /// </summary>
    public interface ISitemapSourceCacheManager
    {
        Task ClearCacheAsync(SitemapCacheContext context);
    }
}
