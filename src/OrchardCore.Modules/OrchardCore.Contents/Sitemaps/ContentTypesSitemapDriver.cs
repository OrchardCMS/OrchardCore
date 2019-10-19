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
    public class ContentTypesSitemapDriver : DisplayDriver<Sitemap, ContentTypesSitemap>
    {
        private readonly IEnumerable<IRouteableContentTypeDefinitionProvider> _routeableContentTypeDefinitionProviders;
        private readonly ISitemapManager _sitemapManager;

        public ContentTypesSitemapDriver(
            IEnumerable<IRouteableContentTypeDefinitionProvider> routeableContentTypeDefinitionProviders,
            ISitemapManager sitemapManager
            )
        {
            _routeableContentTypeDefinitionProviders = routeableContentTypeDefinitionProviders;
            _sitemapManager = sitemapManager;
        }
        public override IDisplayResult Display(ContentTypesSitemap sitemap)
        {
            return Combine(
                View("ContentTypesSitemap_SummaryAdmin", sitemap).Location("SummaryAdmin", "Content"),
                View("ContentTypesSitemap_Thumbnail", sitemap).Location("Thumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(ContentTypesSitemap sitemap)
        {
            var contentTypeDefinitions = _routeableContentTypeDefinitionProviders
                .SelectMany(x => x.ListRoutableTypeDefinitions());

            var entries = contentTypeDefinitions
                .Select(ctd => new ContentTypeSitemapEntryViewModel
                {
                    ContentTypeName = ctd.Name,
                    ContentTypeDisplayName = ctd.DisplayName,
                    IsChecked = sitemap.ContentTypes.Any(selected => selected.ContentTypeName == ctd.Name),
                    ChangeFrequency = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == ctd.Name)?.ChangeFrequency ?? ChangeFrequency.Daily,
                    Priority = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == ctd.Name)?.Priority ?? 5,
                    TakeAll = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == ctd.Name)?.TakeAll ?? true,
                    Skip = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == ctd.Name)?.Skip ?? 0,
                    Take = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == ctd.Name)?.Take ?? 50000,
                })
                .OrderBy(ctd => ctd.ContentTypeDisplayName)
                .ToArray();

            return Initialize<ContentTypesSitemapViewModel>("ContentTypesSitemap_Edit", model =>
            {
                model.Name = sitemap.Name;
                model.Path = sitemap.Path;
                model.IndexAll = sitemap.IndexAll;
                model.Priority = sitemap.Priority;
                model.ChangeFrequency = sitemap.ChangeFrequency;
                model.ContentTypes = entries;
                model.Sitemap = sitemap;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemap sitemap, IUpdateModel updater)
        {
            var model = new ContentTypesSitemapViewModel();

            if (await updater.TryUpdateModelAsync(model,
                Prefix,
                m => m.Name,
                m => m.IndexAll,
                m => m.Priority,
                m => m.ChangeFrequency,
                m => m.Path,
                m => m.ContentTypes))
            {
                sitemap.Name = model.Name;
                sitemap.Path = model.Path;
                sitemap.IndexAll = model.IndexAll;
                sitemap.Priority = model.Priority;
                sitemap.ChangeFrequency = model.ChangeFrequency;
                sitemap.ContentTypes = model.ContentTypes
                    .Where(x => x.IsChecked == true)
                    .Select(x => new ContentTypeSitemapEntry
                    {
                        ContentTypeName = x.ContentTypeName,
                        ChangeFrequency = x.ChangeFrequency,
                        Priority = x.Priority,
                        TakeAll = x.TakeAll,
                        Skip = x.Skip,
                        Take = x.Take
                    })
                    .ToArray();

                if (String.IsNullOrEmpty(sitemap.Path))
                {
                    sitemap.Path = _sitemapManager.GetSitemapSlug(sitemap.Name);
                }

                await _sitemapManager.ValidatePathAsync(sitemap, updater);
            };

            return Edit(sitemap);
        }
    }
}
