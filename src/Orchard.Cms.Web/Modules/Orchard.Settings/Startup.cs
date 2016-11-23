using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Settings.Drivers;
using Orchard.Settings.Recipes;
using Orchard.Settings.Services;
using Orchard.SiteSettings;

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
            services.AddSingleton(new SiteSettingsGroupProvider());
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteSettingsGroupProvider = serviceProvider.GetService<SiteSettingsGroupProvider>();
            var t = serviceProvider.GetService<IStringLocalizer<Startup>>();
            siteSettingsGroupProvider.Add("general", t["General"]);
        }
    }
}
