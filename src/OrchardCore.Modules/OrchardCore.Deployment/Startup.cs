using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Core;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Recipes;
using OrchardCore.Deployment.Steps;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

namespace OrchardCore.Deployment
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeploymentServices();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddScoped<IDisplayManager<DeploymentStep>, DisplayManager<DeploymentStep>>();
            services.AddSingleton<IDeploymentTargetProvider, FileDownloadDeploymentTargetProvider>();

            // Custom File deployment step
            services.AddTransient<IDeploymentSource, CustomFileDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<CustomFileDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, CustomFileDeploymentStepDriver>();

            services.AddSingleton<IIndexProvider, DeploymentPlanIndexProvider>();
            services.AddTransient<IDataMigration, Migrations>();

            services.AddScoped<DeploymentPlanService>();

            services.AddRecipeExecutionStep<DeploymentPlansRecipeStep>();

            services.AddTransient<IDeploymentSource, DeploymentPlanDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<DeploymentPlanDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, DeploymentPlanDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "DeleteStep",
                areaName: "OrchardCore.Deployment",
                pattern: "Deployment/DeploymentPlan/{id}/Step/{stepId}/Delete",
                defaults: new { controller = "Step", action = "Delete" }
            );

            routes.MapAreaControllerRoute(
                name: "ExecutePlan",
                areaName: "OrchardCore.Deployment",
                pattern: "Deployment/DeploymentPlan/{id}/Type/{type}/Execute",
                defaults: new { controller = "DeploymentPlan", action = "Execute" }
            );
        }
    }
}