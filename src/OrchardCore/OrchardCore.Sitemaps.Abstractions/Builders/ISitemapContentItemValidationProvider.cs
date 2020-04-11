using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Validates a content item for inclusion in a sitemap.
    /// </summary>
    public interface ISitemapContentItemValidationProvider
    {
        Task<bool> ValidateContentItem(ContentItem contentItem);
    }
}
