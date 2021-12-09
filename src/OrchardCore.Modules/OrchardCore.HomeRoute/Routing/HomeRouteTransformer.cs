using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly ISiteService _siteService;

        public HomeRouteTransformer(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            var homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;

            if (homeRoute.Count > 0)
            {
                return new RouteValueDictionary(homeRoute);
            }
            else
            {
                return null;
            }
        }
    }
}
