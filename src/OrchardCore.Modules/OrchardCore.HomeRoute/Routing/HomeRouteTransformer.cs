using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    // c.f. https://github.com/aspnet/AspNetCore/issues/12915
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

            if (values == null)
            {
                return new ValueTask<RouteValueDictionary>(new RouteValueDictionary(_routeValues));
            }
            else
            {
                var routeValues = new RouteValueDictionary(values);

                foreach (var entry in _routeValues)
                {
                    routeValues[entry.Key] = entry.Value;
                }

                return new ValueTask<RouteValueDictionary>(routeValues);
            }
        }
    }
}
