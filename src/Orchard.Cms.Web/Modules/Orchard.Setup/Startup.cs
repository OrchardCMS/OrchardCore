using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
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

            // TODO: Remove once Orchard.Cms is a default feature always on
            services.AddSingleton(new DatabaseProvider { Name = "Sql Server", Value = "SqlConnection", HasConnectionString = true });
            services.AddSingleton(new DatabaseProvider { Name = "Sql Lite", Value = "Sqlite", HasConnectionString = false });
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Setup",
                area: "Orchard.Setup",
                template: "",
                controller: "Setup",
                action: "Index"
            );
        }
    }
}
