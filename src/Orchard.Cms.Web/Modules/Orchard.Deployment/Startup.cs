using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Data.Migration;
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
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDeploymentStepDisplayManager, DeploymentStepDisplayManager>();
            services.AddSingleton<IDeploymentTargetProvider, DeploymentTargetProvider>();
            services.AddSingleton<IDeploymentManager, DeploymentManager>();

            services.AddTransient<IDeploymentSource, AllContentDeploymentSource>();
            services.AddTransient<IDeploymentSource, ContentTypeDeploymentSource>();

            services.AddTransient<IDeploymentStepDisplayDriver, ContentTypeDeploymentStepDriver>();
            services.AddTransient<IDeploymentStepDisplayDriver, AllContentDeploymentStepDriver>();

            services.AddTransient<IIndexProvider, DeploymentPlanIndexProvider>();
            services.AddTransient<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DeleteStep",
                area: "Orchard.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Step/{stepId}/Delete",
                controller: "Step",
                action: "Delete"
            );

            routes.MapAreaRoute(
                name: "ExecutePlan",
                area: "Orchard.Deployment",
                template: "Deployment/DeploymentPlan/{id}/Type/{type}/Execute",
                controller: "DeploymentPlan",
                action: "Execute"
            );

            var deploymentTargetProvider = serviceProvider.GetService<IDeploymentTargetProvider>();
            var t = serviceProvider.GetService<IStringLocalizer<Startup>>();

            deploymentTargetProvider.DeploymentTargets.Add(new DeploymentTarget(
                name: t["File Download"],
                description: t["Download a deployment plan locally."],
                route: new RouteValueDictionary(new
                {
                    area = "Orchard.Deployment",
                    controller = "ExportFile",
                    action = "Execute"
                })
            ));
        }
    }
}
