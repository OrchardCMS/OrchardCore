using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteContentRouteProvider : IContentRouteProvider
    {
        private readonly IAutorouteEntries _autorouteEntries;
        private readonly AutorouteOptions _options;

        public AutorouteContentRouteProvider(
            IAutorouteEntries autorouteEntries,
            IOptions<AutorouteOptions> options)
        {
            _autorouteEntries = autorouteEntries;
            _options = options.Value;
        }

        public bool TryGetContentRouteValues(HttpContext httpContext, out RouteValueDictionary routeValues)
        {
            if (_autorouteEntries.TryGetContentItemId(httpContext.Request.Path.Value, out var contentItemId))
            {
                routeValues = new RouteValueDictionary(_options.GlobalRouteValues)
                {
                    [_options.ContentItemIdKey] = contentItemId
                };

                return true;
            }

            routeValues = null;

            return false;
        }

        public bool TryGetContentItemId(string path, out string contentItemId)
        {
            return _autorouteEntries.TryGetContentItemId(path, out contentItemId);
        }
    }
}
