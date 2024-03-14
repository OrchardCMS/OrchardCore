using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Services;

/// <summary>
/// Provides routable content types to the coordinator.
/// </summary>
public interface IRouteableContentTypeProvider
{
    /// <summary>
    /// Provides routable content types.
    /// </summary>
    Task<IEnumerable<ContentTypeDefinition>> ListRoutableTypeDefinitionsAsync();

    /// <summary>
    /// Gets the route for a content item, when building a sitemap.
    /// </summary>
    Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem);

    /// <summary>
    /// Provides routable content types.
    /// </summary>
    [Obsolete($"Instead, utilize the {nameof(ListRoutableTypeDefinitionsAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<ContentTypeDefinition> ListRoutableTypeDefinitions()
        => ListRoutableTypeDefinitionsAsync().GetAwaiter().GetResult();
}
