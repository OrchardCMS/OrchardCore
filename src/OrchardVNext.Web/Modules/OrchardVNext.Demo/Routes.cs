using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Template;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Demo {
    public class Routes : IRouteProvider {
        public readonly ShellSettings _shellSettings;
        public Routes(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                //new RouteDescriptor {
                //    Prefix = "hello/world",
                //    Route = new DelegateRouteEndpoint(async (context) =>
                //                                        await context
                //                                                .HttpContext
                //                                                .Response
                //                                                .WriteAsync("Hello, World! from " + _shellSettings.Name))
                //}
                new RouteDescriptor {
                    Route = new Route(
                        null,
                        "{controller}/{action}",
                        defaults:  new
                            {
                                area = "OrchardVNext.Demo",
                                controller = "Home",
                                action = "Index"
                            },
                        dataTokens:  new
                            {
                                area = "OrchardVNext.Demo"
                            }
                        )

                }
            };
        }
    }
}