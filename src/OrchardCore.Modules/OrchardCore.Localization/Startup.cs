using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Localization.Services;
using OrchardCore.Modules;

namespace OrchardCore.Localization
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override int Order => -10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            // Configure supported cultures and localization options
            services.Configure<RequestLocalizationOptions>(options =>
            {
                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header
                //options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Insert(0, new DefaultRequestCultureProvider());
            });
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var cultureManager = serviceProvider.GetService<ICultureManager>();
            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;

            if (cultureManager != null)
            {
                var siteCultures = cultureManager.ListCultures().Select(c => c.Culture).ToArray();

                options.SetDefaultCulture(cultureManager.GetSiteCulture());
                options.AddSupportedCultures(siteCultures);
                options.AddSupportedUICultures(siteCultures);
            }

            app.UseRequestLocalization(options);
        }
    }
}
