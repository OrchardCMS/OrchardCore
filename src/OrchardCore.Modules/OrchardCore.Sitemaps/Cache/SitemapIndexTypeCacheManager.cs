using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Cache
{
    public class SitemapIndexTypeCacheManager : ISitemapTypeCacheManager
    {
        private readonly ISitemapCacheProvider _sitemapCacheProvider;

        public SitemapIndexTypeCacheManager(ISitemapCacheProvider sitemapCacheProvider)
        {
            _sitemapCacheProvider = sitemapCacheProvider;
        }

        public async Task ClearCacheAsync(SitemapCacheContext context)
        {
            var contentItem = context.CacheObject as ContentItem;

            var sitemapIndex = context.Sitemaps
                .FirstOrDefault(s => s.GetType() == typeof(SitemapIndex));

            if (contentItem == null || sitemapIndex == null)
            {
                return;
            }

            var contentTypeName = contentItem.ContentType;

            var sitemaps = context.Sitemaps.OfType<Sitemap>();

            foreach (var sitemap in sitemaps)
            {
                foreach (var source in sitemap.SitemapSources.Select(x => x as ContentTypesSitemapSource))
                {
                    if (source == null)
                    {
                        continue;
                    }

                    if (source.IndexAll)
                    {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemapIndex.Path);
                        return;
                    }
                    else if (source.LimitItems && String.Equals(source.LimitedContentType.ContentTypeName, contentTypeName))
                    {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemapIndex.Path);
                        return;
                    }
                    else if (source.ContentTypes.Any(ct => String.Equals(ct.ContentTypeName, contentTypeName)))
                    {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemapIndex.Path);
                        return;
                    }
                }
            }
        }
    }
}
