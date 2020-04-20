using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Handlers;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceUpdateHandler : ISitemapSourceUpdateHandler
    {
        private readonly ISitemapManager _sitemapManager;

        public ContentTypesSitemapSourceUpdateHandler(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            var contentItem = context.UpdatedObject as ContentItem;

            if (contentItem == null)
            {
                return;
            }

            var sitemaps = (await _sitemapManager.LoadSitemapsAsync())
                .Where(s => s.GetType() == typeof(Sitemap));

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

                    if (source.IndexAll)
                    {
                        await _sitemapManager.UpdateSitemapAsync(sitemap);
                        break;
                    }
                    else if (source.LimitItems && String.Equals(source.LimitedContentType.ContentTypeName, contentTypeName))
                    {
                        await _sitemapManager.UpdateSitemapAsync(sitemap);
                        break;
                    }
                    else if (source.ContentTypes.Any(ct => String.Equals(ct.ContentTypeName, contentTypeName)))
                    {
                        await _sitemapManager.UpdateSitemapAsync(sitemap);
                        break;
                    }
                }
            }
        }
    }
}
