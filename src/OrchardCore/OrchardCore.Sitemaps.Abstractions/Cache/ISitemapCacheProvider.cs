using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    /// <summary>
    /// Provides sitemap files from the cache.
    /// </summary>
    public interface ISitemapCacheProvider
    {
        Task<ISitemapCacheFileResolver> GetCachedSitemapAsync(string cacheFileName);
        Task SetSitemapCacheAsync(Stream stream, string cacheFileName, CancellationToken cancellationToken);
        Task CleanSitemapCacheAsync(IEnumerable<string> validCacheFileNames);
        Task ClearSitemapCacheAsync(string cacheFileName);
        Task<bool> PurgeAllAsync();
        Task<bool> PurgeAsync(string cacheFileName);
        Task<IEnumerable<string>> ListAsync();
    }
}
