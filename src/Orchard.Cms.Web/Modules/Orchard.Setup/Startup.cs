using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data;
using Orchard.Setup.Services;

namespace Orchard.Setup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISetupService, SetupService>();

            services.TryAddDataProvider(name: "Sql Server", value: "SqlConnection", hasConnectionString: true );
            services.TryAddDataProvider(name: "Sql Lite", value: "Sqlite", hasConnectionString: false );
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Setup",
                areaName: "Orchard.Setup",
                template: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}
