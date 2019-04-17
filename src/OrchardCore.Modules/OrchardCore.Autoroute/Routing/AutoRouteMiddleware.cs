using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;

namespace OrchardCore.Autoroute.Routing
{
    public class AutoRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAutorouteEntries _entries;
        private readonly EndpointDataSource _endpointDataSource;

        public AutoRouteMiddleware(RequestDelegate next, IAutorouteEntries entries, EndpointDataSource endpointDataSource)
        {
            _next = next;
            _entries = entries;
            _endpointDataSource = endpointDataSource;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_entries.TryGetContentItemId(httpContext.Request.Path.ToString().TrimEnd('/'), out var contentItemId))
            {
                var autoRoute = httpContext.RequestServices.GetRequiredService<AutoRoute>();
                var routeValues = await autoRoute.GetValuesAsync(contentItemId);

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
