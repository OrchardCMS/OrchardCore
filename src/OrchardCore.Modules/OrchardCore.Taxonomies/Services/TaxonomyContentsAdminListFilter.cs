using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Services
{
    public class TaxonomyContentsAdminListFilter : IContentsAdminListFilter
    {
        private readonly ISiteService _siteService;

        public TaxonomyContentsAdminListFilter(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();
            foreach (var contentItemId in settings.TaxonomyContentItemIds)
            {
                var viewModel = new TaxonomyContentsAdminFilterViewModel();
                if (await updater.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId))
                {
                    // Show all items categorized by the taxonomy
                    if (!String.IsNullOrEmpty(viewModel.SelectedContentItemId))
                    {
                        if (viewModel.SelectedContentItemId.StartsWith("Taxonomy:", StringComparison.OrdinalIgnoreCase))
                        {
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(9);
                            query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == viewModel.SelectedContentItemId);
                        }
                        else if (viewModel.SelectedContentItemId.StartsWith("Term:", StringComparison.OrdinalIgnoreCase))
                        {
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(5);
                            query.With<TaxonomyIndex>(x => x.TermContentItemId == viewModel.SelectedContentItemId);
                        }
                    }
                }
            }
        }
    }
}
