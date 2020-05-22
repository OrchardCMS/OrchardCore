using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Services
{
    public interface IContentAdminRouteValueProvider
    {
        Task ProvideRouteValuesAsync(IUpdateModel updateModel, RouteValueDictionary routeValueDictionary);
    }
}
