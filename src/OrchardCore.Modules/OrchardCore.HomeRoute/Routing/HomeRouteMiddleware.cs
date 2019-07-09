using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteMiddleware
    {
        private readonly HomeRoute _homeRoute;
        private readonly RequestDelegate _next;
        private readonly EndpointDataSource _endpointDataSource;

        public HomeRouteMiddleware(RequestDelegate next, HomeRoute homeRoute, EndpointDataSource endpointDataSource)
        {
            _next = next;
            _homeRoute = homeRoute;
            _endpointDataSource = endpointDataSource;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.ToString().TrimEnd('/') == String.Empty)
            {
                var routeValues = await _homeRoute.GetValuesAsync();

                if (routeValues != null)
                {
                    var endpoint = _endpointDataSource.Endpoints
                        .Where(e => e.Match(routeValues))
                        .FirstOrDefault();

                    if (endpoint != null)
                    {
                        endpoint.Select(httpContext, routeValues);
                    }
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}
