using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

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
                        .Where(e => Match(e, routeValues))
                        .FirstOrDefault();

                    if (endpoint != null)
                    {
                        var context = new EndpointSelectorContext()
                        {
                            Endpoint = endpoint,
                            RouteValues = routeValues
                        };

                        httpContext.Features.Set<IRoutingFeature>(context);
                        httpContext.Features.Set<IRouteValuesFeature>(context);
                        httpContext.Features.Set<IEndpointFeature>(context);
                    }
                }
            }

            await _next.Invoke(httpContext);
        }

        private bool Match(Endpoint endpoint, RouteValueDictionary routeValues)
        {
            var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (descriptor == null)
            {
                return false;
            }

            return
                String.Equals(descriptor.RouteValues["area"], routeValues["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(descriptor.ControllerName, routeValues["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(descriptor.ActionName, routeValues["action"]?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
