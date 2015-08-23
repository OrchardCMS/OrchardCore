using System.Collections.Generic;
using OrchardVNext.Hosting.Web.Routing.Routes;

namespace OrchardVNext.Demo {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "1",
                        "Home/Index",
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
                },
                new RouteDescriptor {
                    Route = new Route(
                        "2",
                        "Home/IndexError",
                        defaults:  new
                            {
                                area = "OrchardVNext.Demo",
                                controller = "Home",
                                action = "IndexError"
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