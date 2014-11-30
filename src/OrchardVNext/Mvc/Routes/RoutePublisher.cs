using Microsoft.AspNet.Routing;
using OrchardVNext.Environment.Configuration;
using System.Collections.Generic;

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

        public void Publish(IEnumerable<RouteDescriptor> routes) {
            foreach (var route in routes) {

                _routeBuilder.AddPrefixRoute(_shellSettings.RequestUrlPrefix, route.Prefix, route.Route);
            }
        }
    }
}