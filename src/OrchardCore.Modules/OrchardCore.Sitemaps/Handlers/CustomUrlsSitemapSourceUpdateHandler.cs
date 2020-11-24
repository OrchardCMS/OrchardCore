using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Handlers
{
    public class CustomUrlsSitemapSourceUpdateHandler : ISitemapSourceUpdateHandler
    {
        private readonly ISitemapManager _sitemapManager;

        public CustomUrlsSitemapSourceUpdateHandler(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            var contentItem = context.UpdateObject as ContentItem;

            if (contentItem == null)
            {
                return;
            }

            var sitemaps = (await _sitemapManager.LoadSitemapsAsync())
                .Where(s => s.GetType() == typeof(Sitemap));

            if (!sitemaps.Any())
            {
                return;
            }

            var contentTypeName = contentItem.ContentType;

            foreach (var sitemap in sitemaps)
            {
                // Do not break out of this loop, as it must check each sitemap.
                foreach (var source in sitemap.SitemapSources
                    .Select(s => s as CustomUrlSitemapSource))
                {
                    if (source == null)
                    {
                        continue;
                    }

                    sitemap.Identifier = IdGenerator.GenerateId();
                }
            }

            await _sitemapManager.UpdateSitemapAsync();
        }
    }
}
