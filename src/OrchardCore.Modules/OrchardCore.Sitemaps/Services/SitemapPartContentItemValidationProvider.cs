using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapPartContentItemValidationProvider : ISitemapPartContentItemValidationProvider
    {
        public Task<bool> ValidateContentItem(ContentItem contentItem)
        {
            // SitemapPart is optional.
            // To exclude or override defaults add it to the ContentItem.
            var sitemapPart = contentItem.As<SitemapPart>();
            if (sitemapPart != null && sitemapPart.OverrideSitemapConfig && sitemapPart.Exclude)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
