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

namespace OrchardCore.Contents.Sitemaps
{
    public class DefaultContentItemsQueryProvider : IContentItemsQueryProvider
    {
        private readonly ISession _session;
        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;

        public DefaultContentItemsQueryProvider(
            ISession session,
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator
            )
        {
            _session = session;
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
        }

        public async Task GetContentItemsAsync(ContentTypesSitemapSource source, ContentItemsQueryContext context)
        {
            var routeableContentTypeDefinitions = _routeableContentTypeCoordinator.ListRoutableTypeDefinitions();

            if (source.IndexAll)
            {
                var rctdNames = routeableContentTypeDefinitions.Select(rctd => rctd.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(rctdNames))
                    .OrderBy(x => x.CreatedUtc)
                    .ListAsync();

                context.ContentItems = queryResults;
            }
            else if (source.LimitItems)
            {
                // Test that content type is still valid to include in sitemap.
                var typeIsValid = routeableContentTypeDefinitions
                    .Any(ctd => String.Equals(source.LimitedContentType.ContentTypeName, ctd.Name));

                if (typeIsValid)
                {
                    var queryResults = await _session.Query<ContentItem>()
                        .With<ContentItemIndex>(x => x.ContentType == source.LimitedContentType.ContentTypeName && x.Published)
                        .OrderBy(x => x.CreatedUtc)
                        .Skip(source.LimitedContentType.Skip)
                        .Take(source.LimitedContentType.Take)
                        .ListAsync();

                    context.ContentItems = queryResults;
                }
            }
            else
            {
                // Test that content types are still valid to include in sitemap.
                var typesToIndex = routeableContentTypeDefinitions
                    .Where(ctd => source.ContentTypes.Any(s => String.Equals(ctd.Name, s.ContentTypeName)))
                    .Select(x => x.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                    .OrderBy(x => x.CreatedUtc)
                    .ListAsync();

                context.ContentItems = queryResults;
            }
        }
    }
}
