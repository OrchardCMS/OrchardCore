using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.Apis.GraphQL;
using GraphQL.Types;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISetupService, SetupService>();

            services.AddScoped<ISchema, SiteSetupSchema>();

            services.AddScoped<SiteQuery>();
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
