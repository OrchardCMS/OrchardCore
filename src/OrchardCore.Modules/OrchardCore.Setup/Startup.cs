using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSetup();
		}

		public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Setup",
                areaName: "OrchardCore.Setup",
                template: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}
