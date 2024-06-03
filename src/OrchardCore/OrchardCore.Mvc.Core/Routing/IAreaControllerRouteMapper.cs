using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.Routing
{
    public interface IAreaControllerRouteMapper
    {
        int Order { get; }
        bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor);

        public static string ReplaceMvcPlaceholders(string name, string area, string controller, string action) =>
            name?
                .Replace("{area}", area)
                .Replace("{controller}", controller)
                .Replace("{action}", action);

        public static (string Area, string Controller, string Action) GetMvcRouteValues(ControllerActionDescriptor descriptor)
        {
            var area = descriptor.RouteValues["area"];
            var controller = descriptor.ControllerName;
            var action = descriptor.ActionName;

            return (area, controller, action);
        }
    }
}
