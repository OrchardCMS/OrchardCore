using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.SecureSocketsLayer.Models;

namespace OrchardCore.SecureSocketsLayer.Services
{
    public interface ISecureSocketsLayerService
    {
        //bool ShouldBeSecure(string actionName, string controllerName, RouteValueDictionary routeValues);
        //bool ShouldBeSecure(RequestContext requestContext);
        //bool ShouldBeSecure(ActionExecutingContext actionContext);
        //string InsecureActionUrl(string actionName, string controllerName);
        //string InsecureActionUrl(string actionName, string controllerName, object routeValues);
        //string InsecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues);
        //string SecureActionUrl(string actionName, string controllerName);
        //string SecureActionUrl(string actionName, string controllerName, object routeValues);
        //string SecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues);
        Task<SslSettings> GetSettingsAsync();

        Task<bool> ShouldBeSecureAsync(AuthorizationFilterContext filterContext);
    }
}
