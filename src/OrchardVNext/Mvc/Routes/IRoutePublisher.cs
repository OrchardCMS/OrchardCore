using Microsoft.AspNet.Builder;
using System.Collections.Generic;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Mvc.Routes {
    public interface IRoutePublisher : IDependency {
        void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline);
    }
}