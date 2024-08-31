using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Sitemaps;

public class DefaultContentItemsQueryProvider : IContentItemsQueryProvider
{
    private readonly ISession _session;
    private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;

    public DefaultContentItemsQueryProvider(
        ISession session,
        IRouteableContentTypeCoordinator routeableContentTypeCoordinator)
    {
        _session = session;
        _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
    }

    public async Task GetContentItemsAsync(ContentTypesSitemapSource source, ContentItemsQueryContext context, int? skip = null, int? take = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var routeableContentTypeDefinitions = await _routeableContentTypeCoordinator.ListRoutableTypeDefinitionsAsync();


        var query = _session.Query<ContentItem, ContentItemIndex>();

        if (source.IndexAll)
        {
            var rctdNames = routeableContentTypeDefinitions.Select(rctd => rctd.Name);

            query = query.Where(x => x.Published && x.ContentType.IsIn(rctdNames));
        }
        else if (source.LimitItems)
        {
            // Test that content type is still valid to include in sitemap.
            var typeIsValid = routeableContentTypeDefinitions
                .Any(ctd => string.Equals(source.LimitedContentType.ContentTypeName, ctd.Name, StringComparison.Ordinal));

            if (!typeIsValid)
            {
                return;
            }

            query = query.Where(x => x.ContentType == source.LimitedContentType.ContentTypeName && x.Published);
        }
        else
        {
            // Test that content types are still valid to include in sitemap.
            var typesToIndex = routeableContentTypeDefinitions
                .Where(ctd => source.ContentTypes.Any(s => string.Equals(ctd.Name, s.ContentTypeName, StringComparison.Ordinal)))
                .Select(x => x.Name);

            query = query.Where(x => x.ContentType.IsIn(typesToIndex) && x.Published);
        }

        context.ContentItems = await query
            .OrderBy(x => x.CreatedUtc)
            .ThenBy(x => x.Id)
            .Take(take ?? 0)
            .Skip(skip ?? 0)
            .ListAsync();

        foreach (var contentItem in context.ContentItems)
        {
            // Free up memory.
            _session.Detach(contentItem);
        }
    }
}
