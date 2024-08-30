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

namespace OrchardCore.ContentLocalization.Sitemaps;

public class LocalizedContentItemsQueryProvider : IContentItemsQueryProvider
{
    private readonly IStore _store;
    private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;
    private readonly ILocalizationService _localizationService;

    public LocalizedContentItemsQueryProvider(
        IStore store,
        IRouteableContentTypeCoordinator routeableContentTypeCoordinator,
        ILocalizationService localizationService
        )
    {
        _store = store;
        _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
        _localizationService = localizationService;
    }

    public async Task GetContentItemsAsync(ContentTypesSitemapSource source, ContentItemsQueryContext context, int? skip = null, int? take = null)
    {
        var routeableContentTypeDefinitions = await _routeableContentTypeCoordinator.ListRoutableTypeDefinitionsAsync();
        using var session = _store.CreateSession(withTracking: false);

        IEnumerable<ContentItem> contentItems = null;

        if (source.IndexAll)
        {
            // Assumption here is that at least one content type will be localized.
            var ctdNames = routeableContentTypeDefinitions.Select(ctd => ctd.Name);

            contentItems = await session.Query<ContentItem>()
                .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(ctdNames))
                .OrderBy(x => x.CreatedUtc)
                .ThenBy(x => x.Id)
                .Skip(skip ?? 0)
                .Take(take ?? 0)
                .ListAsync();

        }
        else if (source.LimitItems)
        {
            // Test that content type is still valid to include in sitemap.
            var contentType = routeableContentTypeDefinitions
                .FirstOrDefault(ctd => string.Equals(source.LimitedContentType.ContentTypeName, ctd.Name, StringComparison.Ordinal));

            if (contentType == null)
            {
                return;
            }

            if (contentType.Parts.Any(ctd => string.Equals(ctd.Name, nameof(LocalizationPart), StringComparison.Ordinal)))
            {
                // When limiting items Content item is valid if it is for the default culture.
                var defaultCulture = await _localizationService.GetDefaultCultureAsync();

                // Get all content items here for reference. Then reduce by default culture.
                // We know that the content item should be localized.
                // If it doesn't have a localization part, the content item should have been saved.
                contentItems = await session.Query<ContentItem>()
                    .With<ContentItemIndex>(ci => ci.ContentType == source.LimitedContentType.ContentTypeName && ci.Published)
                    .OrderBy(ci => ci.CreatedUtc)
                    .ThenBy(ci => ci.Id)
                    .With<LocalizedContentItemIndex>(x => x.Culture == defaultCulture)
                    .Take(take ?? 0)
                    .Skip(skip ?? 0)
                    .ListAsync();
            }
            else
            {
                // Content type is not localized. Produce standard results.
                contentItems = await session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType == source.LimitedContentType.ContentTypeName && x.Published)
                    .OrderBy(x => x.CreatedUtc)
                    .Skip(skip ?? 0)
                    .Take(take ?? 0)
                    .ListAsync();

            }
        }
        else
        {
            // Test that content types are still valid to include in sitemap.
            var typesToIndex = routeableContentTypeDefinitions
                .Where(ctd => source.ContentTypes.Any(s => string.Equals(ctd.Name, s.ContentTypeName, StringComparison.Ordinal)))
                .Select(x => x.Name);

            // No advantage here in reducing with localized index.

            contentItems = await session.Query<ContentItem>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                .OrderBy(x => x.CreatedUtc)
                .ThenBy(x => x.Id)
                .Skip(skip ?? 0)
                .Take(take ?? 0)
                .ListAsync();

        }

        if (contentItems != null)
        {
            context.ContentItems = contentItems;

            // Provide all content items with localization as reference content items.
            context.ReferenceContentItems = contentItems.Where(ci => ci.Has<LocalizationPart>());
        }
    }
}
