using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Localization.Drivers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Localization
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, LocalizationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteSettings = serviceProvider.GetService<ISiteService>().GetSiteSettingsAsync().GetAwaiter().GetResult();
            var localizationSettings = siteSettings.As<LocalizationSettings>();

            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;
            options.SetDefaultCulture(localizationSettings.DefaultCulture ?? "");
            options
                .AddSupportedCultures(localizationSettings.SupportedCultures)
                .AddSupportedUICultures(localizationSettings.SupportedCultures)
                ;

            app.UseRequestLocalization(options);
        }
    }
}
