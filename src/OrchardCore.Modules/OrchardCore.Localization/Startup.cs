using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Settings;
using System.Linq;
using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteSettings = serviceProvider.GetService<ISiteService>().GetSiteSettingsAsync().GetAwaiter().GetResult();

            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;

            // If no specific default culture is defined, use the system language by not calling SetDefaultCulture
            // TODO change with ILocalCulture.GetDefaultCulture service or something like it.
            if (!String.IsNullOrEmpty(siteSettings.Culture))
            {
                options.SetDefaultCulture(siteSettings.Culture);
            }

            var supportedCulture = siteSettings.SupportedCultures.ToList();
            supportedCulture.Add(CultureInfo.InstalledUICulture.Name); // TODO needs to be changed with service call
            supportedCulture = supportedCulture.Distinct().ToList();

            if (siteSettings.SupportedCultures.Length > 0)
            {
                options
                    .AddSupportedCultures(supportedCulture.ToArray())
                    .AddSupportedUICultures(supportedCulture.ToArray());
            }

            app.UseRequestLocalization(options);
        }
    }
}
