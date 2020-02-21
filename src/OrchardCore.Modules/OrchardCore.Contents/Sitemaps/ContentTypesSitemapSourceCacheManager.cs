using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceCacheManager : ISitemapSourceCacheManager
    {
        private readonly ISitemapCacheProvider _sitemapCacheProvider;

        public ContentTypesSitemapSourceCacheManager(ISitemapCacheProvider sitemapCacheProvider)
        {
            _sitemapCacheProvider = sitemapCacheProvider;
        }

        public async Task ClearCacheAsync(SitemapCacheContext context)
        {
            var contentItem = context.CacheObject as ContentItem;
            var sitemaps = context.Sitemaps
                .Where(s => s.GetType() == typeof(Sitemap));

            if (contentItem == null)
            {
                return;
            }

            var contentTypeName = contentItem.ContentType;

            foreach (var sitemap in sitemaps)
            {
                // Do not break out of this loop, as it must check each sitemap.
                foreach (var source in sitemap.SitemapSources
                    .Select(s => s as ContentTypesSitemapSource))
                {
                    if (source == null)
                    {
                        continue;
                    }

                    if (source.IndexAll) {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);
                        break;
                    }
                    else if (source.LimitItems && String.Equals(source.LimitedContentType.ContentTypeName, contentTypeName))
                    {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);
                        break;
                    }
                    else if (source.ContentTypes.Any(ct => String.Equals(ct.ContentTypeName, contentTypeName)))
                    {
                        await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);
                        break;
                    }
                }
            }
        }
    }
}
