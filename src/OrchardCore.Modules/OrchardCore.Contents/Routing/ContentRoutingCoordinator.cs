using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Contents.Routing
{
    public class ContentRoutingCoordinator : IContentRoutingCoordinator
    {
        private readonly IEnumerable<IContentRouteProvider> _contentRouteProviders;

        public ContentRoutingCoordinator(IEnumerable<IContentRouteProvider> contentRouteProviders)
        {
            _contentRouteProviders = contentRouteProviders;
        }

        public bool TryGetContentRouteValues(HttpContext httpContext, out RouteValueDictionary routeValues)
        {
            foreach (var contentRouteProvider in _contentRouteProviders)
            {
                if(contentRouteProvider.TryGetContentRouteValues(httpContext, out routeValues)){
                    return true;
                }
            }

            routeValues = null;

            return false;
        }

        public bool TryGetContentItemId(string path, out string contentItemId)
        {
            foreach (var contentRouteProvider in _contentRouteProviders)
            {
                if(contentRouteProvider.TryGetContentItemId(path, out contentItemId))
                {
                    return true;
                }
            }

            contentItemId = null;
            return false;
        }
    }
}
