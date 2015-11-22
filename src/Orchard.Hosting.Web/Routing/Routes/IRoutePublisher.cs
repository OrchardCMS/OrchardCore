using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Orchard.DependencyInjection;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public interface IRoutePublisher : ISingletonDependency
    {
        void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline);
    }
}