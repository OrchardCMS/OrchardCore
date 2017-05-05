using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Localization.Core;
using System.Globalization;

namespace Orchard.Localization
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
            services.AddOrchardLocalization(options => options.ResourcesPath = "Localization");
        }
    }
}
