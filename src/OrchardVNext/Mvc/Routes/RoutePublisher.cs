using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Framework.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Mvc.Routes {
    public class RoutePublisher : IRoutePublisher {
        private readonly IRouteBuilder _routeBuilder;
        private readonly ShellSettings _shellSettings;

        public RoutePublisher(
            IRouteBuilder routeBuilder,
            ShellSettings shellSettings) {
            _routeBuilder = routeBuilder;
            _shellSettings = shellSettings;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline) {
            var routesArray = routes
                .OrderByDescending(r => r.Priority)
                .ToArray();

            foreach (var route in routesArray) {
                
                IRouter router = new TemplateRoute(
                    _routeBuilder.DefaultHandler,
                    route.Route.RouteName,
                    route.Route.RouteTemplate,
                    route.Route.Defaults,
                    route.Route.Constraints,
                    route.Route.DataTokens,
                    _routeBuilder.ServiceProvider.GetService<IInlineConstraintResolver>());

                _routeBuilder.AddTenantRoute(_shellSettings.RequestUrlHost, router, pipeline);

            }
        }
    }
}