using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Liquid;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.Drivers;
using OrchardCore.Queries.Liquid;
using OrchardCore.Queries.Recipes;
using OrchardCore.Queries.Services;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Scripting;
using OrchardCore.Admin;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using OrchardCore.Queries.Controllers;

namespace OrchardCore.Queries
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IQueryManager, QueryManager>();
            services.AddScoped<IDisplayManager<Query>, DisplayManager<Query>>();

            services.AddScoped<IDisplayDriver<Query>, QueryDisplayDriver>();
            services.AddRecipeExecutionStep<QueryStep>();
            services.AddScoped<IPermissionProvider, Permissions>();


            services.AddTransient<IDeploymentSource, AllQueriesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllQueriesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllQueriesDeploymentStepDriver>();
            services.AddSingleton<IGlobalMethodProvider, QueryGlobalMethodProvider>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "QueriesIndex",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Index",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesCreate",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Create/{id}",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesDelete",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Delete/{id}",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesEdit",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Edit/{id}",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesRunSql",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Sql/Query",
                defaults: new { controller = typeof(OrchardCore.Queries.Sql.Controllers.AdminController).ControllerName(), action = nameof(OrchardCore.Queries.Sql.Controllers.AdminController.Query) }
            );
        }
    }


    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ILiquidTemplateEventHandler, QueriesLiquidTemplateEventHandler>();

            services.AddLiquidFilter<QueryFilter>("query");
        }
    }
}
