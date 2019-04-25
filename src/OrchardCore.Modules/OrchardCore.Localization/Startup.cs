using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Localization.Drivers;
using OrchardCore.Localization.Services;
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
            services.AddScoped<ILocalizationService, LocalizationService>();

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider.GetService<ILocalizationService>();

            var defaultCulture = localizationService.GetDefaultCultureAsync().GetAwaiter().GetResult();
            var supportedCultures = localizationService.GetSupportedCulturesAsync().GetAwaiter().GetResult();

            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;
            options.SetDefaultCulture(defaultCulture);
            options
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures)
                ;

            app.UseRequestLocalization(options);
        }

        private async Task<ISite> GetSiteSettingsAsync(IServiceProvider serviceProvider)
        {
            var shellHost = serviceProvider.GetRequiredService<IShellHost>();
            var currentShellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            ISite siteSettings = null;

            if (!currentShellSettings.Name.Equals(ShellHelper.DefaultShellName) && currentShellSettings.State == TenantState.Uninitialized)
            {
                using (var serviceScope = await shellHost.GetScopeAsync(ShellHelper.DefaultShellName))
                {
                    siteSettings = await serviceScope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
                }
            }
            else
            {
                siteSettings = await serviceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
            }

            return siteSettings;
        }
    }
}