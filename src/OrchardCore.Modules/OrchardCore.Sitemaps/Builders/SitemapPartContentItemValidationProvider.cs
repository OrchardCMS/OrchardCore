using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapPartContentItemValidationProvider : ISitemapPartContentItemValidationProvider
    {
        public Task<bool> ValidateContentItem(ContentItem contentItem)
        {
            var sitemapPart = contentItem.As<SitemapPart>();
            if (sitemapPart != null && sitemapPart.OverrideSitemapConfig && sitemapPart.Exclude)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
