using Microsoft.AspNet.Routing;
using System;

namespace OrchardVNext.Mvc.Routes {
    public static class RouteBuilderExtensions {
        public static IRouteBuilder AddTenantRoute(this IRouteBuilder routeBuilder,
                                                   string urlHost,
                                                   IRouter handler) {
            routeBuilder.Routes.Add(new TenantRoute(handler, urlHost));
            return routeBuilder;
        }
    }
}