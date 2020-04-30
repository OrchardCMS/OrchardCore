using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    public class ExportContentToDeploymentTargetContentDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model)
        {
            //TODO sort with 20.2 to group better
            return Shape("ExportContentToDeploymentTarget_SummaryAdmin__Button__Actions", new ContentItemViewModel(model)).Location("SummaryAdmin", "ActionsMenu:30");
        }
    }
}
