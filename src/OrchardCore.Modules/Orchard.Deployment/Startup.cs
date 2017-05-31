using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Deployment.Core;
using Orchard.Deployment.Indexes;
using Orchard.Deployment.Steps;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using YesSql.Indexes;

namespace Orchard.Deployment
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeploymentServices();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayManager<DeploymentStep>, DisplayManager<DeploymentStep>>();
            services.AddSingleton<IDeploymentTargetProvider, FileDownloadDeploymentTargetProvider>();

            services.AddTransient<IDeploymentSource, AllContentDeploymentSource>();
            services.AddTransient<IDeploymentSource, CustomFileDeploymentSource>();
            services.AddTransient<IDeploymentSource, ContentTypeDeploymentSource>();

            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ContentTypeDeploymentStep>());
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllContentDeploymentStep>());
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<CustomFileDeploymentStep>());

            services.AddScoped<IDisplayDriver<DeploymentStep>, AllContentDeploymentStepDriver>();
            services.AddScoped<IDisplayDriver<DeploymentStep>, ContentTypeDeploymentStepDriver>();
            services.AddScoped<IDisplayDriver<DeploymentStep>, CustomFileDeploymentStepDriver>();

            services.AddTransient<IIndexProvider, DeploymentPlanIndexProvider>();
            services.AddTransient<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DeleteStep",
                areaName: "Orchard.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Step/{stepId}/Delete",
                defaults: new { controller = "Step", action = "Delete" }
            );

            routes.MapAreaRoute(
                name: "ExecutePlan",
                areaName: "Orchard.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Type/{type}/Execute",
                defaults: new { controller = "DeploymentPlan", action = "Execute" }
            );
        }
    }
}
