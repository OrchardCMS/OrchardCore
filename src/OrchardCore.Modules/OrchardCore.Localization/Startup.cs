using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data.Migration;
using OrchardCore.Localization.Indexes;
using OrchardCore.Localization.Services;
using OrchardCore.Modules;
using OrchardCore.Modules.Services;
using YesSql.Indexes;

namespace OrchardCore.Localization
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteCulture = serviceProvider.GetService<ILocalCulture>().GetLocalCultureAsync().Result;
            var siteCultures = serviceProvider.GetService<ICultureManager>().ListCultures();
            IList<CultureInfo> supportedCultures = new List<CultureInfo>();

            foreach (var culture in siteCultures)
            {
                supportedCultures.Add(new CultureInfo(culture.Culture));
            }

            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(siteCulture),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            base.Configure(app, routes, serviceProvider);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            // Override the default localization file locations with Orchard specific ones
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, CultureIndexProvider>();
            services.AddScoped<ICultureStore, CultureStore>();
            services.AddScoped<ICultureManager, CultureManager>();
        }
    }
}
