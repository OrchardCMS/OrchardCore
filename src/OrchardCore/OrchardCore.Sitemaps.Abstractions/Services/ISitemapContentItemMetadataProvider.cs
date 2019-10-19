using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Services
{
    //TODO consider moving RouteProviders into here, as they develop further for decoupled.
    public interface ISitemapContentItemMetadataProvider
    {
        bool ValidateContentItem(ContentItem contentItem);
        string GetChangeFrequency(ContentItem contentItem);
        int? GetPriority(ContentItem contentItem);
    }
}
