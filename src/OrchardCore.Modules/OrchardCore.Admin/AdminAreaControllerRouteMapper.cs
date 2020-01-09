using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Routing;
using OrchardCore.Mvc.Routing;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private readonly string _defaultUnmappedAreaPattern;
        private const string _defaultMappedAreaPattern = "/{controller}/{action}/{id?}";

        private readonly ShellRouteOptions _shellRouteOptions;
        private readonly AdminOptions _adminOptions;

        public int Order => -1000;

        public AdminAreaControllerRouteMapper(IOptions<AdminOptions> adminOptions, IOptions<ShellRouteOptions> shellRouteOptions)
        {
            _defaultUnmappedAreaPattern = adminOptions.Value.AdminUrlPrefix + "/{area}/{controller}/{action}/{id?}";
            _shellRouteOptions = shellRouteOptions.Value;
            _adminOptions = adminOptions.Value;
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            if (descriptor.ControllerName == "Admin" ||
                descriptor.ControllerTypeInfo.GetCustomAttribute<AdminAttribute>() != null ||
                descriptor.MethodInfo.GetCustomAttribute<AdminAttribute>() != null
                )
            {
                string pattern;
                if (_shellRouteOptions.AdminAreaRouteMap.TryGetValue(descriptor.RouteValues["area"], out var mappedArea))
                {
                    pattern = _adminOptions.AdminUrlPrefix + '/' + mappedArea + _defaultMappedAreaPattern;
                }
                else
                {
                    pattern = _defaultUnmappedAreaPattern;
                }

                routes.MapAreaControllerRoute(
                    name: descriptor.DisplayName,
                    areaName: descriptor.RouteValues["area"],
                    pattern: pattern,
                    defaults: new { controller = descriptor.ControllerName, action = descriptor.ActionName }
                );

                return true;
            }

            return false;
        }
    }
}
