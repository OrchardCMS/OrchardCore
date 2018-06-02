using System;
using Microsoft.AspNetCore.Builder;
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
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                //TODO set supported cultures
            });

            base.Configure(app, routes, serviceProvider);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(
                ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            services.AddScoped<ICultureStore, CultureStore>();
            services.AddScoped<ICultureManager, CultureManager>();
            services.AddScoped<ILocalCulture, LocalCulture>();
        }
    }
}
