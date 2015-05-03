using System.Collections.Generic;
using OrchardVNext.Mvc.Routes;

namespace OrchardVNext.Demo {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        null,
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

                }
            };
        }
    }
}