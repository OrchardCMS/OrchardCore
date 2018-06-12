using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Core;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Steps;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
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

            services.AddTransient<IDeploymentSource, AllContentDeploymentSource>();
            services.AddTransient<IDeploymentSource, CustomFileDeploymentSource>();
            services.AddTransient<IDeploymentSource, ContentDeploymentSource>();

            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ContentDeploymentStep>());
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllContentDeploymentStep>());
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<CustomFileDeploymentStep>());

            services.AddScoped<IDisplayDriver<DeploymentStep>, AllContentDeploymentStepDriver>();
            services.AddScoped<IDisplayDriver<DeploymentStep>, ContentDeploymentStepDriver>();
            services.AddScoped<IDisplayDriver<DeploymentStep>, CustomFileDeploymentStepDriver>();

            services.AddSingleton<IIndexProvider, DeploymentPlanIndexProvider>();
            services.AddTransient<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DeleteStep",
                areaName: "OrchardCore.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Step/{stepId}/Delete",
                defaults: new { controller = "Step", action = "Delete" }
            );

            routes.MapAreaRoute(
                name: "ExecutePlan",
                areaName: "OrchardCore.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Type/{type}/Execute",
                defaults: new { controller = "DeploymentPlan", action = "Execute" }
            );
        }
    }
}
