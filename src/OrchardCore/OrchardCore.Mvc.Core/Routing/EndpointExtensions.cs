using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.Routing
{
    public static class EndpointExtensions
    {
        public static bool MatchControllerRoute(this Endpoint endpoint, RouteValueDictionary routeValues)
        {
            var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (descriptor == null)
            {
                return false;
            }

            foreach (var entry in descriptor.RouteValues)
            {
                if (!String.Equals(routeValues[entry.Key]?.ToString(), entry.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
