using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Deployment.Core;
using Orchard.Deployment.Editors;
using Orchard.Deployment.Indexes;
using Orchard.Deployment.Services;
using Orchard.Deployment.Steps;
using Orchard.Environment.Navigation;
using YesSql.Core.Indexes;

namespace Orchard.Deployment
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeploymentServices();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDeploymentStepDisplayManager, DeploymentStepDisplayManager>();
            services.AddSingleton<IDeploymentTargetProvider, FileDownloadDeploymentTargetProvider>();

            services.AddTransient<IDeploymentSource, AllContentDeploymentSource>();
            services.AddTransient<IDeploymentSource, CustomFileDeploymentSource>();
            services.AddTransient<IDeploymentSource, ContentTypeDeploymentSource>();

            services.AddTransient<IDeploymentStepDisplayDriver, ContentTypeDeploymentStepDriver>();
            services.AddTransient<IDeploymentStepDisplayDriver, CustomFileDeploymentStepDriver>();
            services.AddTransient<IDeploymentStepDisplayDriver, AllContentDeploymentStepDriver>();

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
