using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class StartupConfigure : StartupBase
    {
        public override int Order => 200;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // The default route is added to each tenant as a template route.
            routes.MapRoute("Default", "{area:exists}/{controller}/{action}/{id?}");
        }
    }
}
