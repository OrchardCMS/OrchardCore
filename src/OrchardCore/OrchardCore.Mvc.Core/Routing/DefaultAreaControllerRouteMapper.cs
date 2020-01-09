using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Routing;

namespace OrchardCore.Mvc.Routing
{
    public class DefaultAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private const string _defaultAreaPattern = "/{area}/{controller}/{action}/{id?}";
        private const string _defaultMappedAreaPattern = "/{controller}/{action}/{id?}";

        private readonly ShellRouteOptions _shellRouteOptions;
        public int Order => 1000;

        public DefaultAreaControllerRouteMapper(IOptions<ShellRouteOptions> shellRouteOptions)
        {
            _shellRouteOptions = shellRouteOptions.Value;
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            string pattern;
            if (_shellRouteOptions.AreaRouteMap.TryGetValue(descriptor.RouteValues["area"], out var mappedArea))
            {
                pattern = mappedArea + _defaultMappedAreaPattern;
            }
            else
            {
                pattern = _defaultAreaPattern;
            }

            routes.MapAreaControllerRoute(
               name: descriptor.DisplayName,
               areaName: descriptor.RouteValues["area"],
               pattern: pattern,
               defaults: new { controller = descriptor.ControllerName, action = descriptor.ActionName }
            );

            return true;
        }
    }
}
