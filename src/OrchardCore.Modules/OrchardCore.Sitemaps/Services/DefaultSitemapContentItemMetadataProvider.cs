using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class DefaultSitemapContentItemMetadataProvider : ISitemapContentItemMetadataProvider
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

        public bool ValidateContentItem(ContentItem contentItem)
        {
            // SitemapPart is optional, but to exclude or override defaults add it to the ContentItem
            var sitemapPart = contentItem.As<SitemapPart>();
            if (sitemapPart != null && sitemapPart.OverrideSitemapConfig && sitemapPart.Exclude)
            {
                return false;
            }

            return true;
        }
    }
}
