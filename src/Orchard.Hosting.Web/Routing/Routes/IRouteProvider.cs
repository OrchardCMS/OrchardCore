using System.Collections.Generic;
using Orchard.DependencyInjection;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public interface IRouteProvider : ISingletonDependency
    {
        /// <summary>
        /// obsolete, prefer other format for extension methods
        /// </summary>
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}