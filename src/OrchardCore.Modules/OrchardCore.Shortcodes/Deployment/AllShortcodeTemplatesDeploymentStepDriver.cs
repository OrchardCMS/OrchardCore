using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodeTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllShortcodeTemplatesDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(AllShortcodeTemplatesDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("AllShortcodeTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllShortcodeTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(AllShortcodeTemplatesDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("AllShortcodeTemplatesDeploymentStep_Edit", step).Location("Content")
            );
        }
    }
}
