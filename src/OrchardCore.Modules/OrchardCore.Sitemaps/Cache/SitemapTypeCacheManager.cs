using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    public class SitemapTypeCacheManager : ISitemapTypeCacheManager
    {
        private readonly IEnumerable<ISitemapSourceCacheManager> _sitemapSourceCacheManagers;

        public SitemapTypeCacheManager(IEnumerable<ISitemapSourceCacheManager> sitemapSourceCacheManagers)
        {
            _sitemapSourceCacheManagers = sitemapSourceCacheManagers;
        }

        public async Task ClearCacheAsync(SitemapCacheContext context)
        {
            foreach (var sitemapSourceCacheManager in _sitemapSourceCacheManagers)
            {
                await sitemapSourceCacheManager.ClearCacheAsync(context);
            }
        }
    }
}
