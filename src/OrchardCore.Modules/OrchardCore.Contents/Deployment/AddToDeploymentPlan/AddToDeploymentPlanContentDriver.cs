using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

public sealed class AddToDeploymentPlanContentDriver : ContentDisplayDriver
{
    private readonly IDeploymentPlanService _deploymentPlanService;

    public AddToDeploymentPlanContentDriver(IDeploymentPlanService deploymentPlanService)
    {
        _deploymentPlanService = deploymentPlanService;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
    {
        return CombineAsync(
                Dynamic("AddToDeploymentPlan_Modal__ActionDeploymentPlan")
                    .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:30")
                    .RenderWhen(static (deploymentPlanService) => deploymentPlanService.DoesUserHavePermissionsAsync(), _deploymentPlanService),
                Factory("AddToDeploymentPlan_SummaryAdmin__Button__Actions", static (ContentItem m) => new ContentItemViewModel(m), model)
                    .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:30")
                    .RenderWhen(static (deploymentPlanService) => deploymentPlanService.DoesUserHavePermissionsAsync(), _deploymentPlanService)
            );
    }
}
