using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentAdminRouteValueProvider : IContentAdminRouteValueProvider
    {
        public async Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var model = new ViewModels.ContentOptions();
            if (await updateModel.TryUpdateModelAsync(model, "Options"))
            {
                routeValueDictionary.TryAdd("Options.OrderBy", model.OrderBy);
                routeValueDictionary.TryAdd("Options.ContentsStatus", model.ContentsStatus);
                routeValueDictionary.TryAdd("Options.SelectedContentType", model.SelectedContentType);
                routeValueDictionary.TryAdd("Options.DisplayText", model.DisplayText);
            }
        }
    }
}
