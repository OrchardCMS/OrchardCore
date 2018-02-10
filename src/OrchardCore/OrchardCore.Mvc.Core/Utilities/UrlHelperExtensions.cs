using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Core.Utilities
{
    public static class UrlHelperExtensions
    {
        public static string ToAbsoluteAction(this IUrlHelper url, string actionName, string controllerName, object routeValues = null)
        {
            return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        public static string GetBaseUrl(this IUrlHelper url)
        {
            var request = url.ActionContext.HttpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host.ToUriComponent();
            return $"{scheme}://{host}";
        }

        public static string ToAbsoluteUrl(this IUrlHelper url, string virtualPath)
        {
            var baseUrl = url.GetBaseUrl();
            var path = url.Content(virtualPath);
            return $"{baseUrl}{path}";
        }
    }
}
