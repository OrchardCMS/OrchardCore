using System.Collections.Generic;
using OrchardVNext.Hosting.Web.Routing.Routes;

namespace OrchardVNext.Setup {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        null,
                        "{controller}/{action}",
                        defaults: new {
                            area = "OrchardVNext.Setup",
                            controller = "Setup",
                            action = "Index"
                        },
                        constraints: new {
                            area = "OrchardVNext.Setup",
                            controller = "Setup"
                        },
                        dataTokens: new {
                            area = "OrchardVNext.Setup"
                        })
                }
            };
        }
    }
}
