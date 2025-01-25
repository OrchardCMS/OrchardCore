using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

[Feature("OrchardCore.Contents.Deployment.AddToDeploymentPlan")]
public sealed class AddToDeploymentPlanStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<ContentItemDeploymentSource, ContentItemDeploymentStep, ContentItemDeploymentStepDriver>();
        services.AddScoped<IContentDisplayDriver, AddToDeploymentPlanContentDriver>();
        services.AddDisplayDriver<ContentOptionsViewModel, AddToDeploymentPlanContentsAdminListDisplayDriver>();
    }
}
