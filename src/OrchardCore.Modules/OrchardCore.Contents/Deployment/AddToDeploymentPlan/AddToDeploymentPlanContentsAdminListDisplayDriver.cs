using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

public sealed class AddToDeploymentPlanContentsAdminListDisplayDriver : DisplayDriver<ContentOptionsViewModel>
{
    private readonly IDeploymentPlanService _deploymentPlanService;

    public AddToDeploymentPlanContentsAdminListDisplayDriver(IDeploymentPlanService deploymentPlanService)
    {
        _deploymentPlanService = deploymentPlanService;
    }

    public override async Task<IDisplayResult> DisplayAsync(ContentOptionsViewModel model, BuildDisplayContext context)
    {
        if (await _deploymentPlanService.DoesUserHavePermissionsAsync())
        {
            return Combine(
                Dynamic("AddToDeploymentPlan__Button__ContentsBulkActions").Location("BulkActions", "Content:20"),
                Dynamic("AddToDeploymentPlan_Modal__ContentsBulkActionsDeploymentPlan").Location("BulkActions", "Content:20")
            );
        }

        return null;
    }
}
