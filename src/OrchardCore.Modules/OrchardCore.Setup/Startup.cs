using System;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Setup.Apis.GraphQL;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISetupService, SetupService>();

            services.AddScoped<ISchema, SiteSetupSchema>();

            services.AddScoped<SiteSetupMutation>();
            services.AddScoped<SiteSetupOutcomeType>();
            services.AddScoped<SiteSetupInputType>();
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
