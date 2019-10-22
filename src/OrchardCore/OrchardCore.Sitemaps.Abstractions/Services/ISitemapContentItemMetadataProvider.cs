using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapContentItemMetadataProvider
    {
        string GetChangeFrequency(ContentItem contentItem);
        int? GetPriority(ContentItem contentItem);
    }
}
