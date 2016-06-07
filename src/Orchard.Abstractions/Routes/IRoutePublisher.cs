using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Orchard.Routes
{
    public interface IRoutePublisher
    {
        void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline);
    }
}