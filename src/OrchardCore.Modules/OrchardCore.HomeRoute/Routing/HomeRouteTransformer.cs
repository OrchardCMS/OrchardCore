using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly ISiteService _siteService;
        private RouteValueDictionary _routeValues;
        private IChangeToken _changeToken;

        public HomeRouteTransformer(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (_changeToken?.HasChanged ?? true)
            {
                var changeToken = _siteService.ChangeToken;
                _routeValues = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult().HomeRoute;
                _changeToken = changeToken;
            }

            var routeValues = new RouteValueDictionary(_routeValues);

            if (values != null)
            {
                foreach (var entry in values)
                {
                    routeValues.TryAdd(entry.Key, entry.Value);
                }
            }

            return new ValueTask<RouteValueDictionary>(routeValues);
        }
    }
}
