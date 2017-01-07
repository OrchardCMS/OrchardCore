using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;

namespace Orchard.Admin
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigation();

            services.AddScoped<IFilterMetadata, AdminZoneFilter>();
            services.AddScoped<IFilterMetadata, AdminFilter>();
            services.AddScoped<IFilterMetadata, AdminMenuFilter>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, AdminThemeSelector>();
            services.AddScoped<IAdminThemeService, AdminThemeService>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Admin",
                areaName: "Orchard.Admin",
                template: "admin",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
