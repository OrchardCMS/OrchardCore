using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    public class AddToDeploymentPlanContentDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model)
        {
            return Shape("AddToDeploymentPlan_SummaryAdmin__Button__Actions", new ContentItemViewModel(model)).Location("SummaryAdmin", "ActionsMenu:25");
        }
    }
}
