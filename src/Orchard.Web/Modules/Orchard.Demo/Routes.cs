using System.Collections.Generic;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Demo
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
            new RouteDescriptor {
                    Route = new Route(
                        "1",
                        "Home/Index",
                        defaults:  new
                            {
                                area = "Orchard.Demo",
                                controller = "Home",
                                action = "Index"
                            },
                        constraints:  new
                            {
                                area = "Orchard.Demo"
                            }
                        )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "1",
                        "Home/Display/{id}",
                        defaults:  new
                            {
                                area = "Orchard.Demo",
                                controller = "Home",
                                action = "Display",
                            },
                        constraints:  new
                            {
                                area = "Orchard.Demo"
                            }
                        )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "2",
                        "Home/IndexError",
                        defaults:  new
                            {
                                area = "Orchard.Demo",
                                controller = "Home",
                                action = "IndexError"
                            },
                        constraints:  new
                            {
                                area = "Orchard.Demo"
                            }
                        )
                }
            };
        }
    }
}