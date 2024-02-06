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
            services.AddDeployment<ContentItemDeploymentSource, ContentItemDeploymentStep, ContentItemDeploymentStepDriver>();
            services.AddScoped<IContentDisplayDriver, AddToDeploymentPlanContentDriver>();
            services.AddScoped<IDisplayDriver<ContentOptionsViewModel>, AddToDeploymentPlanContentsAdminListDisplayDriver>();
        }
    }
}
