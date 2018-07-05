using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Liquid;
using OrchardCore.Localization.Indexes;
using OrchardCore.Localization.Services;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Drivers;
using OrchardCore.Settings.Recipes;
using OrchardCore.Settings.Services;
using OrchardCore.Setup.Events;
using YesSql.Indexes;

namespace OrchardCore.Settings
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
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
            services.AddScoped<ICultureStore, CultureStore>();
            services.AddScoped<ICultureManager, CultureManager>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, CultureIndexProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaRoute(
                name: "AdminSettings",
                areaName: "OrchardCore.Settings",
                template: "Admin/Settings/{groupId}",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }
    }
}
