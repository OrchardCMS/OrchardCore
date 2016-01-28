using System.Collections.Generic;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Contents
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "DisplayContent",
                        "Contents/Item/Display/{id}",
                        defaults:  new
                            {
                                area = "Orchard.Contents",
                                controller = "Item",
                                action = "Display"
                            },
                        dataTokens:  new
                            {
                                area = "Orchard.Contents"
                            }
                        )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "PreviewContent",
                        "Contents/Item/Preview/{id}",
                        defaults:  new
                            {
                                area = "Orchard.Contents",
                                controller = "Item",
                                action = "Preview"
                            },
                        dataTokens:  new
                            {
                                area = "Orchard.Contents"
                            }
                        )
                }
            };
        }
    }
}