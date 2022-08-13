using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Entities;
using OrchardCore.Search.Model;
using OrchardCore.Settings;

namespace OrchardCore.Search.Routing
{
    public class SearchRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly ISiteService _siteService;

        public SearchRouteTransformer(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            var site = await _siteService.GetSiteSettingsAsync();
            var settings = site.As<SearchSettings>();

            var routeValue = new RouteValueDictionary( new { Name = "Search", Area = "OrchardCore.Search." + settings.SearchProvider, Action = "Search", Controller = "Search" });

            if (routeValue.Count > 0)
            {
                return new RouteValueDictionary(routeValue);
            }
            else
            {
                return null;
            }
        }
    }
}
