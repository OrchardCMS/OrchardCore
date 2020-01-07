using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Routing;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private readonly string _pattern;

        public AdminAreaControllerRouteMapper(IOptions<AdminOptions> adminOptions)
        {
            _pattern = adminOptions.Value.AdminUrlPrefix + "/{area}/{controller}/{action}/{id?}";
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            if (descriptor.ControllerName == "Admin" || descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AdminAttribute), false).Any())
            {
                routes.MapAreaControllerRoute(
                    name: descriptor.DisplayName,
                    areaName: descriptor.RouteValues["area"],
                    pattern: _pattern,
                    defaults: new { controller = descriptor.ControllerName, action = descriptor.ActionName }
                );

                return true;
            }

            return false;
        }
    }
}
