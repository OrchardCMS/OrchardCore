using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISiteService _siteService;
        private readonly EndpointDataSource _endpointDataSource;
        private RouteValueDictionary _routeValues;
        private IChangeToken _changeToken;

        public HomeRouteMiddleware(
            RequestDelegate next,
            ISiteService siteService,
            EndpointDataSource endpointDataSource)
        {
            _next = next;
            _siteService = siteService;
            _endpointDataSource = endpointDataSource;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.ToString().TrimEnd('/') == String.Empty)
            {
                if (_changeToken?.HasChanged ?? true)
                {
                    var changeToken = _siteService.ChangeToken;
                    _routeValues = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
                    _changeToken = changeToken;
                }

                var endpoint = _endpointDataSource.Endpoints
                    .Where(e => e.MatchControllerRoute(_routeValues))
                    .FirstOrDefault();

                if (endpoint != null)
                {
                    endpoint.Select(httpContext, new RouteValueDictionary(_routeValues));
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}
