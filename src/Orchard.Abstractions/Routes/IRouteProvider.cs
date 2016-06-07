using System.Collections.Generic;

namespace Orchard.Routes
{
    public interface IRouteProvider
    {
        /// <summary>
        /// obsolete, prefer other format for extension methods
        /// </summary>
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}