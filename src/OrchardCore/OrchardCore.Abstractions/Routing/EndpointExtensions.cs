using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Routing
{
    public static class EndpointExtensions
    {
        public static void Select(this Endpoint endpoint, HttpContext httpContext, RouteValueDictionary routeValues)
        {
            var routingFeature = new RoutingFeature()
            {
                RouteData = new RouteData(routeValues)
            };

            var routeValuesFeature = new RouteValuesFeature()
            {
                RouteValues = routeValues
            };

            var endpointFeature = new EndpointFeature()
            {
                Endpoint = endpoint,
            };

            httpContext.Features.Set<IRoutingFeature>(routingFeature);
            httpContext.Features.Set<IRouteValuesFeature>(routeValuesFeature);
            httpContext.Features.Set<IEndpointFeature>(endpointFeature);
        }
    }
}
