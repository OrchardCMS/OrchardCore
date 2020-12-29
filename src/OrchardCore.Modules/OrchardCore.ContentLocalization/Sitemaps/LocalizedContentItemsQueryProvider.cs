using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Localization;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentLocalization.Sitemaps
{
    public class LocalizedContentItemsQueryProvider : IContentItemsQueryProvider
    {
        private readonly ISession _session;
        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;
        private readonly ILocalizationService _localizationService;

        public LocalizedContentItemsQueryProvider(
            ISession session,
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator,
            ILocalizationService localizationService
            )
        {
            _session = session;
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
            _localizationService = localizationService;
        }

        public async Task GetContentItemsAsync(ContentTypesSitemapSource source, ContentItemsQueryContext queryContext)
        {
            var routeableContentTypeDefinitions = _routeableContentTypeCoordinator.ListRoutableTypeDefinitions();

            if (source.IndexAll)
            {
                // Assumption here is that at least one content type will be localized.
                var rctdNames = routeableContentTypeDefinitions.Select(rctd => rctd.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(rctdNames))
                    .OrderBy(x => x.CreatedUtc)
                    .ListAsync();

                queryContext.ContentItems = queryResults;

                // Provide all content items with localization as reference content items.
                queryContext.ReferenceContentItems = queryResults
                    .Where(ci => ci.Has<LocalizationPart>());
            }
            else if (source.LimitItems)
            {
                // Test that content type is still valid to include in sitemap.
                var contentType = routeableContentTypeDefinitions
                    .FirstOrDefault(ctd => String.Equals(source.LimitedContentType.ContentTypeName, ctd.Name));

                if (contentType == null)
                {
                    return;
                }

                if (contentType.Parts.Any(ctd => String.Equals(ctd.Name, nameof(LocalizationPart))))
                {
                    // Get all content items here for reference. Then reduce by default culture.
                    // We know that the content item should be localized.
                    // If it doesn't have a localization part, the content item should have been saved.
                    var queryResults = await _session.Query<ContentItem>()
                         .With<ContentItemIndex>(ci => ci.ContentType == source.LimitedContentType.ContentTypeName && ci.Published)
                         .OrderBy(ci => ci.CreatedUtc)
                         .With<LocalizedContentItemIndex>()
                         .ListAsync();

                    // When limiting items Content item is valid if it is for the default culture.
                    var defaultCulture = await _localizationService.GetDefaultCultureAsync();

                    // Reduce by default culture.
                    var items = queryResults
                        .Where(ci => String.Equals(ci.As<LocalizationPart>().Culture, defaultCulture))
                        .Skip(source.LimitedContentType.Skip)
                        .Take(source.LimitedContentType.Take);

                    queryContext.ContentItems = items;

                    // Provide all content items with localization as reference content items.
                    queryContext.ReferenceContentItems = queryResults
                        .Where(ci => ci.Has<LocalizationPart>());
                }
                else
                {
                    // Content type is not localized. Produce standard results.
                    var queryResults = await _session.Query<ContentItem>()
                        .With<ContentItemIndex>(x => x.ContentType == source.LimitedContentType.ContentTypeName && x.Published)
                        .OrderBy(x => x.CreatedUtc)
                        .Skip(source.LimitedContentType.Skip)
                        .Take(source.LimitedContentType.Take)
                        .ListAsync();

                    queryContext.ContentItems = queryResults;
                }
            }
            else
            {
                // Test that content types are still valid to include in sitemap.
                var typesToIndex = routeableContentTypeDefinitions
                    .Where(ctd => source.ContentTypes.Any(s => String.Equals(ctd.Name, s.ContentTypeName)))
                    .Select(x => x.Name);

                // No advantage here in reducing with localized index.
                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                    .OrderBy(x => x.CreatedUtc)
                    .ListAsync();

                queryContext.ContentItems = queryResults;

                // Provide all content items with localization as reference content items.
                queryContext.ReferenceContentItems = queryResults
                    .Where(ci => ci.Has<LocalizationPart>());
            }
        }
    }
}
