using System.Collections.Generic;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Core.Dashboard
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Orchard.Dashboard",
                        "admin",
                        defaults:  new
                        {
                            area = "Orchard.Dashboard",
                            controller = "Admin",
                            action = "Index"
                        },
                        constraints: new
                        {
                            area = "Orchard.Dashboard"
                        })
                }
            };
        }
    }
}