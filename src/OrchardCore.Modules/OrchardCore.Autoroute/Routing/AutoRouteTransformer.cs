using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Routing
{
    public class AutoRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly IAutorouteEntries _entries;
        private readonly AutorouteOptions _options;

        public AutoRouteTransformer(IAutorouteEntries entries, IOptions<AutorouteOptions> options)
        {
            _entries = entries;
            _options = options.Value;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (_entries.TryGetContentItemId(httpContext.Request.Path.ToString().TrimEnd('/'), out var contentItemId))
            {
                var routeValues = new RouteValueDictionary(GetRouteValues(contentItemId));

                if (values != null)
                {
                    foreach (var entry in values)
                    {
                        routeValues.TryAdd(entry.Key, entry.Value);
                    }
                }

                return new ValueTask<RouteValueDictionary>(routeValues);
            }

            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary(_options.GlobalRouteValues));
        }

        private RouteValueDictionary GetRouteValues(string contentItemId)
        {
            var routeValues = new RouteValueDictionary(_options.GlobalRouteValues);
            routeValues[_options.ContentItemIdKey] = contentItemId;
            return routeValues;
        }
    }
}
