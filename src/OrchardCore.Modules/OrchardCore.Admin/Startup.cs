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
                options.Filters.Add(typeof(AdminFilter));
                options.Filters.Add(typeof(AdminMenuFilter));

                // Ordered to be called before any global filter.
                options.Filters.Add(typeof(AdminZoneFilter), -1000);
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, AdminThemeSelector>();
            services.AddScoped<IAdminThemeService, AdminThemeService>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Admin",
                areaName: "OrchardCore.Admin",
                pattern: "admin",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
