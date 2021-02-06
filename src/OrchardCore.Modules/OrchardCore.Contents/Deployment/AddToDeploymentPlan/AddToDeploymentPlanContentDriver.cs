using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class AddToDeploymentPlanContentDriver : ContentDisplayDriver
    {
        private readonly IDeploymentPlanService _deploymentPlanService;

        public AddToDeploymentPlanContentDriver(IDeploymentPlanService deploymentPlanService)
        {
            _deploymentPlanService = deploymentPlanService;
        }

        public override IDisplayResult Display(ContentItem model)
        {
            return Combine(
                    Dynamic("AddToDeploymentPlan_Modal__ActionDeploymentPlan")
                        .Location("SummaryAdmin", "ActionsMenu:30")
                        .RenderWhen(async () => await _deploymentPlanService.DoesUserHavePermissionsAsync()),
                    Shape("AddToDeploymentPlan_SummaryAdmin__Button__Actions", new ContentItemViewModel(model))
                        .Location("SummaryAdmin", "ActionsMenu:30")
                        .RenderWhen(async () => await _deploymentPlanService.DoesUserHavePermissionsAsync())
                );
        }
    }
}
