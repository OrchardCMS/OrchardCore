using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Demo
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {

            routes.MapAreaRoute(
                name: "Home",
                area: "Orchard.Demo",
                template: "Home/Index",
                controller: "Home",
                action: "Index"
            );

            routes.MapAreaRoute(
                name: "Display",
                area: "Orchard.Demo",
                template: "Home/Display/{id}",
                controller: "Home",
                action: "Display"
            );

            routes.MapAreaRoute(
                name: "Error",
                area: "Orchard.Demo",
                template: "Home/IndexError",
                controller: "Home",
                action: "IndexError"
            );

            builder.UseMiddleware<NonBlockingMiddleware>();
            builder.UseMiddleware<BlockingMiddleware>();
        }
    }
}
