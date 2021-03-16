using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceDriver : DisplayDriver<SitemapSource, ContentTypesSitemapSource>
    {
        private readonly IEnumerable<IRouteableContentTypeProvider> _routeableContentTypeDefinitionProviders;

        public ContentTypesSitemapSourceDriver(
            IEnumerable<IRouteableContentTypeProvider> routeableContentTypeDefinitionProviders
            )
        {
            _routeableContentTypeDefinitionProviders = routeableContentTypeDefinitionProviders;
        }

        public override IDisplayResult Display(ContentTypesSitemapSource sitemapSource)
        {
            return Combine(
                View("ContentTypesSitemapSource_SummaryAdmin", sitemapSource).Location("SummaryAdmin", "Content"),
                View("ContentTypesSitemapSource_Thumbnail", sitemapSource).Location("Thumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(ContentTypesSitemapSource sitemapSource, IUpdateModel updater)
        {
            var contentTypeDefinitions = _routeableContentTypeDefinitionProviders
                .SelectMany(x => x.ListRoutableTypeDefinitions());

            var entries = contentTypeDefinitions
                .Select(ctd => new ContentTypeSitemapEntryViewModel
                {
                    ContentTypeName = ctd.Name,
                    ContentTypeDisplayName = ctd.DisplayName,
                    IsChecked = sitemapSource.ContentTypes.Any(s => String.Equals(s.ContentTypeName, ctd.Name)),
                    ChangeFrequency = sitemapSource.ContentTypes.FirstOrDefault(s => String.Equals(s.ContentTypeName, ctd.Name))?.ChangeFrequency ?? ChangeFrequency.Daily,
                    Priority = sitemapSource.ContentTypes.FirstOrDefault(s => String.Equals(s.ContentTypeName, ctd.Name))?.Priority ?? 5,
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
                .FirstOrDefault(ctd => String.Equals(sitemapSource.LimitedContentType.ContentTypeName, ctd.Name));

            if (limitedCtd != null)
            {
                var limitedEntry = limitedEntries.FirstOrDefault(le => String.Equals(le.ContentTypeName, limitedCtd.Name));
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
                model.LimitedContentType = limitedCtd != null ? limitedCtd.Name : contentTypeDefinitions.FirstOrDefault().Name;
                model.SitemapSource = sitemapSource;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemapSource sitemap, UpdateEditorContext context)
        {
            var model = new ContentTypesSitemapSourceViewModel();

            if (await context.Updater.TryUpdateModelAsync(model,
                    Prefix,
                    m => m.IndexAll,
                    m => m.LimitItems,
                    m => m.Priority,
                    m => m.ChangeFrequency,
                    m => m.ContentTypes,
                    m => m.LimitedContentTypes,
                    m => m.LimitedContentType
                ))
            {
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

                var limitedEntry = model.LimitedContentTypes.FirstOrDefault(lct => String.Equals(lct.ContentTypeName, model.LimitedContentType));
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
            };

            return Edit(sitemap, context.Updater);
        }
    }
}
