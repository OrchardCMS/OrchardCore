using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Services;

public class DefaultRouteableContentTypeCoordinator : IRouteableContentTypeCoordinator
{
    private readonly IEnumerable<IRouteableContentTypeProvider> _routeableContentTypeProviders;

    public DefaultRouteableContentTypeCoordinator(IEnumerable<IRouteableContentTypeProvider> routeableContentTypeProviders)
    {
        _routeableContentTypeProviders = routeableContentTypeProviders;
    }

    public async Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem)
    {
        foreach (var rctp in _routeableContentTypeProviders)
        {
            var result = await rctp.GetRouteAsync(context, contentItem);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public async Task<IEnumerable<ContentTypeDefinition>> ListRoutableTypeDefinitionsAsync()
    {
        var results = new List<ContentTypeDefinition>();

        foreach (var routeableContentTypeProvider in _routeableContentTypeProviders)
        {
            results.AddRange(await routeableContentTypeProvider.ListRoutableTypeDefinitionsAsync());
        }

        return results;
    }
}
