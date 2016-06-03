using System.Collections.Generic;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Admin
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Orchard.Admin",
                        "admin",
                        defaults:  new
                        {
                            area = "Orchard.Admin",
                            controller = "Admin",
                            action = "Index"
                        },
                        constraints: new
                        {
                            area = "Orchard.Admin"
                        })
                }
            };
        }
    }
}