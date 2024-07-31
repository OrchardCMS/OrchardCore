using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    public class ExportContentToDeploymentTargetDeploymentStepDriver : DisplayDriver<DeploymentStep, ExportContentToDeploymentTargetDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(ExportContentToDeploymentTargetDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("ExportContentToDeploymentTargetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ExportContentToDeploymentTargetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(ExportContentToDeploymentTargetDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("ExportContentToDeploymentTargetDeploymentStep_Fields_Edit", step).Location("Content")
            );
        }
    }
}
