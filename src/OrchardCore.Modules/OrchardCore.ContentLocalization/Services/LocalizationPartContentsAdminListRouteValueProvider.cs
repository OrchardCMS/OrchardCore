using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationPartContentsAdminListRouteValueProvider : IContentsAdminListRouteValueProvider
    {
        public async Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var viewModel = new LocalizationContentAdminFilterViewModel();
            if (await updateModel.TryUpdateModelAsync(viewModel, "Localization"))
            {
                if (viewModel.ShowLocalizedContentTypes)
                {
                    routeValueDictionary.TryAdd("Localization.ShowLocalizedContentTypes", viewModel.ShowLocalizedContentTypes);
                }

                if (!string.IsNullOrEmpty(viewModel.SelectedCulture))
                {
                    routeValueDictionary.TryAdd("Localization.SelectedCulture", viewModel.SelectedCulture);
                }
            }
        }
    }
}
