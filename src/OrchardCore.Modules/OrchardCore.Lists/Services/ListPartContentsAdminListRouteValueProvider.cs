using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Services
{
    public class ListPartContentsAdminListRouteValueProvider : IContentsAdminListRouteValueProvider
    {
        public async Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var viewModel = new ListPartContentAdminFilterViewModel();
            if (await updateModel.TryUpdateModelAsync(viewModel, nameof(ListPart)))
            {
                if (viewModel.ShowListContentTypes)
                {
                    routeValueDictionary.TryAdd("ListPart.ShowListContentTypes", viewModel.ShowListContentTypes);
                }

                if (!string.IsNullOrEmpty(viewModel.ListContentItemId))
                {
                    routeValueDictionary.TryAdd("ListPart.ListContentItemId", viewModel.ListContentItemId);
                }
            }
        }

    }
}
