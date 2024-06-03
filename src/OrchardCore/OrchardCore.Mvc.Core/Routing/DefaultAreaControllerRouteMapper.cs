using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using static OrchardCore.Mvc.Routing.IAreaControllerRouteMapper;

namespace OrchardCore.Mvc.Routing
{
    public class DefaultAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private const string DefaultAreaPattern = "/{area}/{controller}/{action}/{id?}";

        public int Order => 1000;

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            var (area, controller, action) = GetMvcRouteValues(descriptor);

            routes.MapAreaControllerRoute(
               name: descriptor.DisplayName,
               areaName: area,
               pattern: ReplaceMvcPlaceholders(DefaultAreaPattern, area, controller, action),
               defaults: new { controller, action }
            );

            return true;
        }
    }
}
