using Microsoft.AspNetCore.Mvc.Controllers;

namespace OrchardCore.Mvc.Routing;

public static class RoutingHelper
{
    public static string ReplaceMvcPlaceholders(string name, string area, string controller, string action) =>
        name?
            .Replace("{area}", area)
            .Replace("{controller}", controller)
            .Replace("{action}", action);

    public static (string Area, string Controller, string Action) GetMvcRouteValues(ControllerActionDescriptor descriptor)
    {
        var area = descriptor.RouteValues["area"];

        return (area, descriptor.ControllerName, descriptor.ActionName);
    }
}
