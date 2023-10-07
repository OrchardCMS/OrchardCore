using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.Routing
{
    public class DefaultAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private const string DefaultAreaPattern = "/{area}/{controller}/{action}/{id?}";

        public int Order => 1000;

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            routes.MapAreaControllerRoute(
               name: descriptor.DisplayName,
               areaName: descriptor.RouteValues["area"],
               pattern: DefaultAreaPattern.Replace("{action}", descriptor.ActionName),
               defaults: new { controller = descriptor.ControllerName, action = descriptor.ActionName }
            );

            return true;
        }
    }
}
