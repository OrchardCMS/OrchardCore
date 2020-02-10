using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Settings.Drivers;
using OrchardCore.Settings.Recipes;
using OrchardCore.Settings.Services;
using OrchardCore.Setup.Events;

namespace OrchardCore.Settings
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISetupEventHandler, SetupEventHandler>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddRecipeExecutionStep<SettingsStep>();
            services.AddSingleton<ISiteService, SiteService>();

            // Site Settings editor
            services.AddScoped<IDisplayManager<ISite>, DisplayManager<ISite>>();
            services.AddScoped<IDisplayDriver<ISite>, DefaultSiteSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<ISite>, ButtonsSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<ILiquidTemplateEventHandler, SiteLiquidTemplateEventHandler>();

            services.AddScoped<ITimeZoneSelector, DefaultTimeZoneSelector>();

            services.AddTransient<IDeploymentSource, SiteSettingsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<SiteSettingsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, SiteSettingsDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaControllerRoute(
                name: "AdminSettings",
                areaName: "OrchardCore.Settings",
                pattern: _adminOptions.AdminUrlPrefix + "/Settings/{groupId}",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
