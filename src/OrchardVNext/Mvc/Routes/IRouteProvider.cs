using System.Collections.Generic;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Mvc.Routes {
    public interface IRouteProvider : IDependency {
        /// <summary>
        /// obsolete, prefer other format for extension methods
        /// </summary>
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}