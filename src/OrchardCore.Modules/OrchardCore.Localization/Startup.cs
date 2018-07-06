using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var options = new RequestLocalizationOptions();

            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - RouteDataRequestCultureProvider, determines the culture information for a request via values in the route data.
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header

                new DefaultRequestCultureProvider { Options = options },
                new QueryStringRequestCultureProvider { Options = options },
                new CookieRequestCultureProvider { Options = options },
                new RouteDataRequestCultureProvider { Options = options },
                new AcceptLanguageHeaderRequestCultureProvider { Options = options }
            };

            var cultureManager = serviceProvider.GetService<ICultureManager>();

            if (cultureManager != null)
            {
                var siteCultures = cultureManager.ListCultures().Select(c => c.Culture).ToArray();

                options
                    .SetDefaultCulture(cultureManager.GetSiteCulture())
                    .AddSupportedCultures(siteCultures)
                    .AddSupportedUICultures(siteCultures);
            }

            app.UseRequestLocalization(options);
        }
    }
}
