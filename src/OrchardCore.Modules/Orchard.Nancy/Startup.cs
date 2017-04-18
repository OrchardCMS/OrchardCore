using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Orchard.Nancy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Nancy
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.ConfigureModules(apb =>
            {
                apb.UseNancyModules();
                apb.UseStaticFilesModules();
            });
        }
    }
}
