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
                        "Dashboard",
                        "admin",
                        defaults:  new
                        {
                            area = "Dashboard",
                            controller = "Admin",
                            action = "Index"
                        },
                        constraints: new
                        {
                            area = "Dashboard"
                        })
                }
            };
        }
    }
}