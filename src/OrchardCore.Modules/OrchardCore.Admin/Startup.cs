using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Admin
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigation();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(AdminZoneFilter));
                options.Filters.Add(typeof(AdminFilter));
                options.Filters.Add(typeof(AdminMenuFilter));
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, AdminThemeSelector>();
            services.AddScoped<IAdminThemeService, AdminThemeService>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Admin",
                areaName: "OrchardCore.Admin",
                template: "admin",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
