using Microsoft.AspNet.Routing;
using System;

namespace OrchardVNext.Mvc.Routes {
    public static class RouteBuilderExtensions {
        public static IRouteBuilder AddPrefixRoute(this IRouteBuilder routeBuilder,
                                                   string urlHost,
                                                   string prefix,
                                                   IRouter handler) {
            routeBuilder.Routes.Add(new PrefixRoute(handler, urlHost, prefix));
            return routeBuilder;
        }
    }
}