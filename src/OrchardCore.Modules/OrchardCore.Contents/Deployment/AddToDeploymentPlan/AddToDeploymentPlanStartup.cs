using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    [Feature("OrchardCore.Contents.Deployment.AddToDeploymentPlan")]
    public class AddToDeploymentPlanStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public AddToDeploymentPlanStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, ContentItemDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ContentItemDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ContentItemDeploymentStepDriver>();
            services.AddScoped<IContentDisplayDriver, AddToDeploymentPlanContentDriver>();
            services.AddScoped<IDisplayDriver<ContentOptionsViewModel>, AddToDeploymentPlanContentsAdminListDisplayDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var addToDeploymentPlanControllerName = typeof(AddToDeploymentPlanController).ControllerName();

            routes.MapAreaControllerRoute(
               name: "AddToDeploymentPlan",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AddToDeploymentPlan/AddContentItem/{deploymentPlanId}",
               defaults: new { controller = addToDeploymentPlanControllerName, action = nameof(AddToDeploymentPlanController.AddContentItem) }
           );

            routes.MapAreaControllerRoute(
               name: "AddToDeploymentPlan",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AddToDeploymentPlan/AddContentItems/{deploymentPlanId}",
               defaults: new { controller = addToDeploymentPlanControllerName, action = nameof(AddToDeploymentPlanController.AddContentItems) }
           );
        }
    }
}
