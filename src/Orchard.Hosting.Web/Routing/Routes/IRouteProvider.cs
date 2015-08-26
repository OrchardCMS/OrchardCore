using System.Collections.Generic;
using Orchard.DependencyInjection;

namespace Orchard.Hosting.Web.Routing.Routes {
    public interface IRouteProvider : IDependency {
        /// <summary>
        /// obsolete, prefer other format for extension methods
        /// </summary>
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}