using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;

namespace OrchardCore.Autoroute.Routing
{
    public class AutoRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAutorouteEntries _entries;
        private readonly AutorouteOptions _options;
        private readonly EndpointDataSource _endpointDataSource;

        public AutoRouteMiddleware(
            RequestDelegate next,
            IAutorouteEntries entries,
            IOptions<AutorouteOptions> options,
            EndpointDataSource endpointDataSource)
        {
            _next = next;
            _entries = entries;
            _options = options.Value;
            _endpointDataSource = endpointDataSource;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_entries.TryGetContentItemId(httpContext.Request.Path.ToString().TrimEnd('/'), out var contentItemId))
            {
                var routeValues = GetRouteValues(contentItemId);

                var endpoint = _endpointDataSource.Endpoints
                    .Where(e => e.MatchControllerRoute(routeValues))
                    .FirstOrDefault();

                if (endpoint != null)
                {
                    endpoint.Select(httpContext, routeValues);
                }
            }

            await _next.Invoke(httpContext);
        }

        private RouteValueDictionary GetRouteValues(string contentItemId)
        {
            var routeValues = new RouteValueDictionary(_options.GlobalRouteValues);
            routeValues[_options.ContentItemIdKey] = contentItemId;
            return routeValues;
        }
    }
}
