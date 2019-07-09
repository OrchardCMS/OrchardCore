using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.Routing
{
    public static class EndpointExtensions
    {
        public static bool Match(this Endpoint endpoint, RouteValueDictionary routeValues)
        {
            var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (descriptor == null)
            {
                return false;
            }

            return
                String.Equals(descriptor.RouteValues["area"], routeValues["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(descriptor.ControllerName, routeValues["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(descriptor.ActionName, routeValues["action"]?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
