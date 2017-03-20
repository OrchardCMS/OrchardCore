using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using OrchardCore.Nancy.Modules;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Nancy
{
    public class Startup : StartupBase
    {
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
