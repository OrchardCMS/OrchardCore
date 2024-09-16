using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents.Sitemaps;

public sealed class ContentTypesSitemapSourceDriver : DisplayDriver<SitemapSource, ContentTypesSitemapSource>
{
    private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;

    public ContentTypesSitemapSourceDriver(IRouteableContentTypeCoordinator routeableContentTypeCoordinator)
    {
        _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentTypesSitemapSource sitemapSource, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ContentTypesSitemapSource_SummaryAdmin", sitemapSource).Location("SummaryAdmin", "Content"),
            View("ContentTypesSitemapSource_Thumbnail", sitemapSource).Location("Thumbnail", "Content")
        );
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypesSitemapSource sitemapSource, BuildEditorContext context)
    {
        var contentTypeDefinitions = await _routeableContentTypeCoordinator.ListRoutableTypeDefinitionsAsync();

        var entries = contentTypeDefinitions
            .Select(ctd => new ContentTypeSitemapEntryViewModel
            {
                ContentTypeName = ctd.Name,
                ContentTypeDisplayName = ctd.DisplayName,
                IsChecked = sitemapSource.ContentTypes.Any(s => string.Equals(s.ContentTypeName, ctd.Name, StringComparison.Ordinal)),
                ChangeFrequency = sitemapSource.ContentTypes.FirstOrDefault(s => string.Equals(s.ContentTypeName, ctd.Name, StringComparison.Ordinal))?.ChangeFrequency ?? ChangeFrequency.Daily,
                Priority = sitemapSource.ContentTypes.FirstOrDefault(s => string.Equals(s.ContentTypeName, ctd.Name, StringComparison.Ordinal))?.Priority ?? 5,
            })
            .OrderBy(ctd => ctd.ContentTypeDisplayName)
            .ToArray();

        var limitedEntries = contentTypeDefinitions
            .Select(ctd => new ContentTypeLimitedSitemapEntryViewModel
            {
                ContentTypeName = ctd.Name,
                ContentTypeDisplayName = ctd.DisplayName
            })
            .OrderBy(ctd => ctd.ContentTypeDisplayName)
            .ToArray();

        var limitedCtd = contentTypeDefinitions
            .FirstOrDefault(ctd => string.Equals(sitemapSource.LimitedContentType.ContentTypeName, ctd.Name, StringComparison.Ordinal));

        if (limitedCtd != null)
        {
            var limitedEntry = limitedEntries.FirstOrDefault(le => string.Equals(le.ContentTypeName, limitedCtd.Name, StringComparison.Ordinal));
            limitedEntry.Priority = sitemapSource.LimitedContentType.Priority;
            limitedEntry.ChangeFrequency = sitemapSource.LimitedContentType.ChangeFrequency;
            limitedEntry.Skip = sitemapSource.LimitedContentType.Skip;
            limitedEntry.Take = sitemapSource.LimitedContentType.Take;
        }

        return Initialize<ContentTypesSitemapSourceViewModel>("ContentTypesSitemapSource_Edit", model =>
        {
            model.IndexAll = sitemapSource.IndexAll;
            model.LimitItems = sitemapSource.LimitItems;
            model.Priority = sitemapSource.Priority;
            model.ChangeFrequency = sitemapSource.ChangeFrequency;
            model.ContentTypes = entries;
            model.LimitedContentTypes = limitedEntries;
            model.LimitedContentType = limitedCtd is null ? contentTypeDefinitions.FirstOrDefault()?.Name : limitedCtd.Name;
            model.SitemapSource = sitemapSource;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemapSource sitemap, UpdateEditorContext context)
    {
        var model = new ContentTypesSitemapSourceViewModel();

        await context.Updater.TryUpdateModelAsync(model,
                Prefix,
                m => m.IndexAll,
                m => m.LimitItems,
                m => m.Priority,
                m => m.ChangeFrequency,
                m => m.ContentTypes,
                m => m.LimitedContentTypes,
                m => m.LimitedContentType
            );

        sitemap.IndexAll = model.IndexAll;
        sitemap.LimitItems = model.LimitItems;
        sitemap.Priority = model.Priority;
        sitemap.ChangeFrequency = model.ChangeFrequency;
        sitemap.ContentTypes = model.ContentTypes
            .Where(x => x.IsChecked == true)
            .Select(x => new ContentTypeSitemapEntry
            {
                ContentTypeName = x.ContentTypeName,
                ChangeFrequency = x.ChangeFrequency,
                Priority = x.Priority,
            })
            .ToArray();

        var limitedEntry = model.LimitedContentTypes.FirstOrDefault(lct => string.Equals(lct.ContentTypeName, model.LimitedContentType, StringComparison.Ordinal));
        if (limitedEntry != null)
        {
            sitemap.LimitedContentType.ContentTypeName = limitedEntry.ContentTypeName;
            sitemap.LimitedContentType.ChangeFrequency = limitedEntry.ChangeFrequency;
            sitemap.LimitedContentType.Priority = limitedEntry.Priority;
            sitemap.LimitedContentType.Skip = limitedEntry.Skip;
            sitemap.LimitedContentType.Take = limitedEntry.Take;
        }
        else
        {
            sitemap.LimitedContentType = new LimitedContentTypeSitemapEntry();
        }

        return await EditAsync(sitemap, context);
    }
}
