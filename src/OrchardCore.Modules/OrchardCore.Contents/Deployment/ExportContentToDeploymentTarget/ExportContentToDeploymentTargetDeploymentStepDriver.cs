using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    public class ExportContentToDeploymentTargetDeploymentStepDriver : DisplayDriver<DeploymentStep, ExportContentToDeploymentTargetDeploymentStep>
    {
        public override IDisplayResult Display(ExportContentToDeploymentTargetDeploymentStep step)
        {
            return
                Combine(
                    View("ExportContentToDeploymentTargetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ExportContentToDeploymentTargetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ExportContentToDeploymentTargetDeploymentStep step)
        {
            return View("ExportContentToDeploymentTargetDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
