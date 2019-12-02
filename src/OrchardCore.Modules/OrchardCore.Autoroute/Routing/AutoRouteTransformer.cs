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
            if (_entries.TryGetContentItemId(httpContext.Request.Path.Value.TrimEnd('/'), out var contentItemId))
            {
                var routeValues = new RouteValueDictionary(_options.GlobalRouteValues);
                routeValues[_options.ContentItemIdKey] = contentItemId;
                return new ValueTask<RouteValueDictionary>(routeValues);
            }

            return new ValueTask<RouteValueDictionary>((RouteValueDictionary)null);
        }
    }
}
