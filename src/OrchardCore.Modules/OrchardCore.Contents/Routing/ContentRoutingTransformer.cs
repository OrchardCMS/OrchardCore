using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Contents.Routing
{
    public class ContentRoutingTransformer : DynamicRouteValueTransformer
    {
        private readonly IContentRoutingCoordinator _contentRoutingCoordinator;

        public ContentRoutingTransformer(IContentRoutingCoordinator contentRoutingCoordinator)
        {
            _contentRoutingCoordinator = contentRoutingCoordinator;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            _contentRoutingCoordinator.TryGetContentRouteValues(httpContext, out var routeValues);

            return new ValueTask<RouteValueDictionary>(routeValues);
        }
    }
}
