using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapCacheHandler : ContentHandlerBase
    {
        private readonly ISitemapCacheManager _sitemapCacheManager;
        private readonly ISitemapManager _sitemapManager;

        public ContentTypesSitemapCacheHandler(
            ISitemapCacheManager sitemapCacheManager,
            ISitemapManager sitemapManager
            )
        {
            _sitemapCacheManager = sitemapCacheManager;
            _sitemapManager = sitemapManager;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            var cacheContext = new SitemapCacheContext
            {
                CacheObject = context.ContentItem,
                Sitemaps = await _sitemapManager.ListSitemapsAsync()
            };

            await _sitemapCacheManager.ClearCacheAsync(cacheContext);
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            var cacheContext = new SitemapCacheContext
            {
                CacheObject = context.ContentItem,
                Sitemaps = await _sitemapManager.ListSitemapsAsync()
            };

            await _sitemapCacheManager.ClearCacheAsync(cacheContext);
        }
    }
}
