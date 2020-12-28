using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Cache
{
    /// <summary>
    /// Provides sitemap files from the cache.
    /// </summary>
    public interface ISitemapCacheProvider
    {
        Task<ISitemapCacheFileResolver> GetCachedSitemapAsync(SitemapType sitemap);
        Task SetSitemapCacheAsync(Stream stream, SitemapType sitemap, CancellationToken cancellationToken);
        Task ClearSitemapCacheAsync(SitemapType sitemap);
        Task<bool> PurgeAllAsync();
        Task<bool> PurgeAsync(string cacheFileName);
        Task CleanupAsync(IEnumerable<SitemapType> sitemaps);
        Task<IEnumerable<string>> ListAsync();
    }
}
