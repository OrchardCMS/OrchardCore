using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Services;

// TODO Create a Terms Index independent of the standard index, which can index taxonomy terms by their display text
// Refer https://github.com/OrchardCMS/OrchardCore/issues/5214
// This could then be migrated to use the filter parser.
public class TaxonomyContentsAdminListFilter : IContentsAdminListFilter
{
    private readonly ISiteService _siteService;

    public TaxonomyContentsAdminListFilter(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
    {
        var settings = await _siteService.GetSettingsAsync<TaxonomyContentsAdminListSettings>();
        foreach (var contentItemId in settings.TaxonomyContentItemIds)
        {
            var viewModel = new TaxonomyContentsAdminFilterViewModel();
            await updater.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId);

            // Show all items categorized by the taxonomy
            if (!string.IsNullOrEmpty(viewModel.SelectedContentItemId))
            {
                if (viewModel.SelectedContentItemId.StartsWith("Taxonomy:", StringComparison.OrdinalIgnoreCase))
                {
                    viewModel.SelectedContentItemId = viewModel.SelectedContentItemId[9..];
                    query.All(
                        x => query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == viewModel.SelectedContentItemId)
                    );
                }
                else if (viewModel.SelectedContentItemId.StartsWith("Term:", StringComparison.OrdinalIgnoreCase))
                {
                    viewModel.SelectedContentItemId = viewModel.SelectedContentItemId[5..];
                    query.All(
                        x => query.With<TaxonomyIndex>(x => x.TermContentItemId == viewModel.SelectedContentItemId)
                    );
                }
            }
        }
    }
}
