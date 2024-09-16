using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.Routing;

public class DefaultAreaControllerRouteMapper : IAreaControllerRouteMapper
{
    private const string DefaultAreaPattern = "/{area}/{controller}/{action}/{id?}";

    public int Order => 1000;

    public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
    {
        var (area, controller, action) = RoutingHelper.GetMvcRouteValues(descriptor);

        routes.MapControllerRoute(
           name: descriptor.DisplayName,
           pattern: RoutingHelper.ReplaceMvcPlaceholders(DefaultAreaPattern, area, controller, action),
           defaults: new { area, controller, action }
        );

        return true;
    }
}
