using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Cache
{
    public class DefaultSitemapCacheManager : ISitemapCacheManager
    {
        private readonly IEnumerable<ISitemapTypeCacheManager> _sitemapTypeCacheManagers;

        public DefaultSitemapCacheManager(IEnumerable<ISitemapTypeCacheManager> sitemapTypeCacheManagers)
        {
            _sitemapTypeCacheManagers = sitemapTypeCacheManagers;
        }

        public async Task ClearCacheAsync(SitemapCacheContext context)
        {
            foreach (var sitemapTypeCacheManager in _sitemapTypeCacheManagers)
            {
                await sitemapTypeCacheManager.ClearCacheAsync(context);
            }
        }
    }
}
