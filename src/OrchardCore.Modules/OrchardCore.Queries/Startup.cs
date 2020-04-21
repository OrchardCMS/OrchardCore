using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Queries.Controllers;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.Drivers;
using OrchardCore.Queries.Liquid;
using OrchardCore.Queries.Recipes;
using OrchardCore.Queries.Services;
using OrchardCore.Recipes;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;

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
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "QueriesIndex",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Index",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesCreate",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Create/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesDelete",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Delete/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesEdit",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Edit/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "QueriesRunSql",
                areaName: "OrchardCore.Queries",
                pattern: _adminOptions.AdminUrlPrefix + "/Queries/Sql/Query",
                defaults: new { controller = typeof(Sql.Controllers.AdminController).ControllerName(), action = nameof(Sql.Controllers.AdminController.Query) }
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
