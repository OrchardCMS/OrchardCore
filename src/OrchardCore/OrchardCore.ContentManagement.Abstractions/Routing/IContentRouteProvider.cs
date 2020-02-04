using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IContentRouteProvider
    {
        bool TryGetContentRouteValues(HttpContext httpContext, out RouteValueDictionary routeValues);
        bool TryGetContentItemId(string path, out string contentItemId);
    }
}
