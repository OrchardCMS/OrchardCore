using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Web.Routing.Routes {
    public static class RouteBuilderExtensions {
        public static IRouteBuilder AddTenantRoute(this IRouteBuilder routeBuilder,
                                                   ShellSettings shellSettings,
                                                   IRouter handler,
                                                   RequestDelegate pipeline) {
            routeBuilder.Routes.Add(new TenantRoute(shellSettings, handler, pipeline));
            return routeBuilder;
        }
    }
}