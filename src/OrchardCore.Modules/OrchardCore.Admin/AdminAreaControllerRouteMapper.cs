using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Routing;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private const string _defaultAreaPattern = "{area}/{controller}/{action}/{id?}";
        private readonly string _adminUrlPrefix;

        public int Order => -1000;

        public AdminAreaControllerRouteMapper(IOptions<AdminOptions> adminOptions)
        {
            _adminUrlPrefix = adminOptions.Value.AdminUrlPrefix;
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            var controllerAttribute = descriptor.ControllerTypeInfo.GetCustomAttribute<AdminAttribute>();
            var actionAttribute = descriptor.MethodInfo.GetCustomAttribute<AdminAttribute>();

            if (descriptor.ControllerName != "Admin" && controllerAttribute == null && actionAttribute == null)
            {
                return false;
            }

            string name = null;
            var pattern = _defaultAreaPattern;

            if (!string.IsNullOrWhiteSpace(actionAttribute?.Template))
            {
                name = actionAttribute.RouteName;
                pattern = actionAttribute.Template;
            }
            else if (!string.IsNullOrWhiteSpace(controllerAttribute?.Template))
            {
                name = controllerAttribute.RouteName;
                pattern = controllerAttribute.Template;
            }

            var area = descriptor.RouteValues["area"];
            var controller = descriptor.ControllerName;
            var action = descriptor.ActionName;

            routes.MapAreaControllerRoute(
                name: name?
                    .Replace("{area}", area)
                    .Replace("{controller}", controller)
                    .Replace("{action}", action) ?? descriptor.DisplayName,
                areaName: area,
                pattern: $"{_adminUrlPrefix}/{pattern.TrimStart('/').Replace("{action}", action)}",
                defaults: new { controller, action }
            );

            return true;
        }
    }
}
