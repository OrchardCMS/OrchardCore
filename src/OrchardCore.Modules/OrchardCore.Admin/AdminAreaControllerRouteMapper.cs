/*
    defines the class "AdminAreaControllerRouteMapper" (<- IAreaControllerRouteMapper)
        which forms the URL pattern (".AdminAreaControllerRouteMapper ()")
        and tries to instantiate it by descriptor (".TryMapAreaControllerRoute ()")
*/

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRouteMapper: IAreaControllerRouteMapper
    {
        // URL pattern
        private readonly string _defaultAreaPattern;

        public int Order => -1000;

        /*
            forms a URL pattern from the argument prefix field and the pattern
        */
        public AdminAreaControllerRouteMapper (IOptions <AdminOptions> adminOptions)
        {
            _defaultAreaPattern = adminOptions.Value.AdminUrlPrefix + "/ {area} / {controller} / {action} / {id?}";
        }

        /*
            will return false for NOT admins
        */
        public bool TryMapAreaControllerRoute (IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            // only for admins
            if (descriptor.ControllerName == "Admin" ||
                descriptor.ControllerTypeInfo.GetCustomAttribute <AdminAttribute> ()! = null ||
                descriptor.MethodInfo.GetCustomAttribute <AdminAttribute> ()! = null
                )
            {
                // forms a route by arguments
                routes.MapAreaControllerRoute (
                    name: descriptor.DisplayName,
                    areaName: descriptor.RouteValues ​​["area"],
                    pattern: _defaultAreaPattern.Replace ("{action}", descriptor.ActionName),
                    defaults: new {controller = descriptor.ControllerName, action = descriptor.ActionName}
                );

                return true;
            }

            return false;
        }
    }
}
