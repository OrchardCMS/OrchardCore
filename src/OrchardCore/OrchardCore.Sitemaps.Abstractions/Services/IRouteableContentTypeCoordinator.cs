using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Services
{
    /// <summary>
    /// Coordinates content types that are routable.
    /// </summary>
    public interface IRouteableContentTypeCoordinator
    {
        /// <summary>
        /// Lists all routable content types.
        /// </summary>
        IEnumerable<ContentTypeDefinition> ListRoutableTypeDefinitions();

        /// <summary>
        /// Gets the route for a content item, when building a sitemap.
        /// </summary>
        Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem);
    }
}
