using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Services
{
    public class TaxonomyContentAdminRouteValueProvider : IContentAdminRouteValueProvider
    {
        private readonly ISiteService _siteService;

        public TaxonomyContentAdminRouteValueProvider(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();
            foreach (var contentItemId in settings.TaxonomyContentItemIds)
            {
                var viewModel = new TaxonomyContentAdminFilterViewModel();
                if (await updateModel.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId))
                {
                    if (!String.IsNullOrEmpty(viewModel.SelectedContentItemId))
                    {
                        routeValueDictionary.TryAdd("Taxonomy" + contentItemId + ".SelectedContentItemId", viewModel.SelectedContentItemId);
                    }
                }
            }
        }
    }
}
