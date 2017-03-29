using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Navigation;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Settings.Drivers;
using Orchard.Settings.Recipes;
using Orchard.Settings.Services;

namespace Orchard.Settings
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<SetupEventHandler>();
            services.AddScoped<ISetupEventHandler>(sp => sp.GetRequiredService<SetupEventHandler>());
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddRecipeExecutionStep<SettingsStep>();
            services.AddScoped<ISiteService, SiteService>();

            // Site Settings editor
            services.AddScoped<ISiteSettingsDisplayManager, SiteSettingsDisplayManager>();
            services.AddScoped<ISiteSettingsDisplayHandler, SiteSettingsDisplayCoordinator>();
            services.AddScoped<ISiteSettingsDisplayDriver, DefaultSiteSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaRoute(
                name: "AdminSettings",
                areaName: "Orchard.Settings",
                template: "Admin/Settings/{groupId}",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
