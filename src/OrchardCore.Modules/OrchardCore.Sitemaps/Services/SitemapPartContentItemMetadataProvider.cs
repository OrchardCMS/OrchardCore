using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapPartContentItemMetadataProvider : ISitemapContentItemMetadataProvider
    {
        public string GetChangeFrequency(ContentItem contentItem)
        {
            var part = contentItem.As<SitemapPart>();
            if (part != null && part.OverrideSitemapConfig)
            {
                return part.ChangeFrequency.ToString();
            }

            return null;
        }

        public int? GetPriority(ContentItem contentItem)
        {
            var part = contentItem.As<SitemapPart>();
            if (part != null && part.OverrideSitemapConfig)
            {
                return part.Priority;
            }

            return null;
        }
    }
}
