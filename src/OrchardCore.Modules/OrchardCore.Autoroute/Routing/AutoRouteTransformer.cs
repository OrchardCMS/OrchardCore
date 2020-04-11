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

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            var contentItemId = await _entries.TryGetContentItemIdAsync(httpContext.Request.Path.Value);

            if (contentItemId == null)
            {
                return null;
            }

            var routeValues = new RouteValueDictionary(_options.GlobalRouteValues);
            routeValues[_options.ContentItemIdKey] = contentItemId;

            return routeValues;
        }
    }
}
