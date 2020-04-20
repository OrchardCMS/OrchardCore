using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Handlers
{
    public class SitemapTypeUpdateHandler : ISitemapTypeUpdateHandler
    {
        private readonly IEnumerable<ISitemapSourceUpdateHandler> _sitemapSourceUpdateManagers;

        public SitemapTypeUpdateHandler(IEnumerable<ISitemapSourceUpdateHandler> sitemapSourceUpdateManagers)
        {
            _sitemapSourceUpdateManagers = sitemapSourceUpdateManagers;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            foreach (var sitemapSourceCacheManager in _sitemapSourceUpdateManagers)
            {
                await sitemapSourceCacheManager.UpdateSitemapAsync(context);
            }
        }
    }
}
