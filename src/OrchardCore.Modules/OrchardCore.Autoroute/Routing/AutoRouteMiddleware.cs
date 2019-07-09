using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;

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
