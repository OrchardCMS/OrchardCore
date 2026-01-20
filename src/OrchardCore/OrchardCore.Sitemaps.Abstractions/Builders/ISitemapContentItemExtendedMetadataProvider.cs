using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Provides extended metadata, such as hreflang, or image sitemap metadata, to a url element.
    /// </summary>
    public interface ISitemapContentItemExtendedMetadataProvider
    {
        /// <summary>
        /// Get extended attributes to add to the xml schema.
        /// </summary>
        XAttribute GetExtendedAttribute { get; }

        /// <summary>
        /// Apply extended metadata to the url element.
        /// </summary>
        Task<bool> ApplyExtendedMetadataAsync(SitemapBuilderContext context, ContentItemsQueryContext queryContext, ContentItem contentItem, XElement url);
    }
}
