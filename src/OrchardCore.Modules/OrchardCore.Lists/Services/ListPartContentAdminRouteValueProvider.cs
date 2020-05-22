using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Services
{
    public class ListPartContentAdminRouteValueProvider : IContentAdminRouteValueProvider
    {
        public async Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var viewModel = new ListPartContentAdminFilterModel();
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
