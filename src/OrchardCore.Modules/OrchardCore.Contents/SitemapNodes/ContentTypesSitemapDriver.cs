using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapDriver : DisplayDriver<Sitemap, ContentTypesSitemap>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypesSitemapDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
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
            // TODO Prefer IsRoutable() to reduce list size, and allow SelectAll
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            //TODO implement IRouteableContentDefinitionProvider in Autoroute.

            var entries = contentTypeDefinitions.Select(x => new ContentTypeSitemapEntryViewModel
            {
                ContentTypeName = x.Name,
                ContentTypeDisplayName = x.DisplayName,
                IsChecked = sitemap.ContentTypes.Any(selected => selected.ContentTypeName == x.Name),
                ChangeFrequency = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.ChangeFrequency ?? ChangeFrequency.Daily,
                Priority = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Priority ?? 0.5f,
                TakeAll = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.TakeAll ?? true,
                Skip = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Skip ?? 0,
                Take = sitemap.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Take ?? 50000,
            }).ToArray();


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
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            sitemap.ContentTypes = Array.Empty<ContentTypeSitemapEntry>();

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
            };

            return Edit(sitemap);
        }
    }
}
