using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Handlers
{
    public class SitemapIndexTypeUpdateHandler : ISitemapTypeUpdateHandler
    {
        private readonly ISitemapManager _sitemapManager;

        public SitemapIndexTypeUpdateHandler(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            var contentItem = context.UpdateObject as ContentItem;

            var allSitemaps = await _sitemapManager.LoadSitemapsAsync();

            var sitemapIndex = allSitemaps
                .FirstOrDefault(s => s.GetType() == typeof(SitemapIndex));

            if (contentItem == null || sitemapIndex == null)
            {
                return;
            }

            var sitemaps = allSitemaps.OfType<Sitemap>();

            if (!sitemaps.Any())
            {
                return;
            }

            var contentTypeName = contentItem.ContentType;

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
                        sitemap.Identifier = IdGenerator.GenerateId();
                        break;
                    }
                    else if (source.LimitItems && String.Equals(source.LimitedContentType.ContentTypeName, contentTypeName))
                    {
                        sitemap.Identifier = IdGenerator.GenerateId();
                        break;
                    }
                    else if (source.ContentTypes.Any(ct => String.Equals(ct.ContentTypeName, contentTypeName)))
                    {
                        sitemap.Identifier = IdGenerator.GenerateId();
                        break;
                    }
                }
            }

            await _sitemapManager.UpdateSitemapAsync();
        }
    }
}
