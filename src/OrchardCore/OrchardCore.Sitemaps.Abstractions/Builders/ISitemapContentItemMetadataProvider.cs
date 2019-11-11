using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Gets change frequency and priotity metadata for a content item url element.
    /// </summary>
    public interface ISitemapContentItemMetadataProvider
    {
        string GetChangeFrequency(ContentItem contentItem);
        int? GetPriority(ContentItem contentItem);
    }
}
