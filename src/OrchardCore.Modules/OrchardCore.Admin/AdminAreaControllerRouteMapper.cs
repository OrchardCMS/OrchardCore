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
        private readonly string _defaultAreaPattern;

        public int Order => -1000;

        public AdminAreaControllerRouteMapper(IOptions<AdminOptions> adminOptions)
        {
            _defaultAreaPattern = adminOptions.Value.AdminUrlPrefix + "/{area}/{controller}/{action}/{id?}";
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            if (descriptor.ControllerName == "Admin" ||
                descriptor.ControllerTypeInfo.GetCustomAttribute<AdminAttribute>() != null ||
                descriptor.MethodInfo.GetCustomAttribute<AdminAttribute>() != null
                )
            {
                routes.MapAreaControllerRoute(
                    name: descriptor.DisplayName,
                    areaName: descriptor.RouteValues["area"],
                    pattern: _defaultAreaPattern.Replace("{action}", descriptor.ActionName),
                    defaults: new { controller = descriptor.ControllerName, action = descriptor.ActionName }
                );

                return true;
            }

            return false;
        }
    }
}
