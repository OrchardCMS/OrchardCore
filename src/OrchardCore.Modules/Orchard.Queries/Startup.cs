using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using Orchard.Liquid;
using Orchard.Queries.Drivers;
using Orchard.Queries.Liquid;
using Orchard.Queries.Recipes;
using Orchard.Queries.Services;
using Orchard.Recipes;
using Orchard.Security.Permissions;

namespace Orchard.Queries
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IQueryManager, QueryManager>();
            services.AddScoped<IDisplayManager<Query>, DisplayManager<Query>>();

            services.AddScoped<IDisplayDriver<Query>, QueryDisplayDriver>();
            services.AddRecipeExecutionStep<QueryStep>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Api.Queries.Query",
                areaName: "Orchard.Queries",
                template: "api/queries/{name}",
                defaults: new { controller = "Api", action = "Query" }
            );
        }
    }


    [RequireFeatures("Orchard.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITemplateContextHandler, QueryFilter>();
        }
    }
}
