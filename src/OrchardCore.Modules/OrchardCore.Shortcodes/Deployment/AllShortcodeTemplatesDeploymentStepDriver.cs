using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodeTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllShortcodeTemplatesDeploymentStep>
    {
        public override IDisplayResult Display(AllShortcodeTemplatesDeploymentStep step)
        {
            return
                Combine(
                    View("AllShortcodeTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllShortcodeTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllShortcodeTemplatesDeploymentStep step)
        {
            return View("AllShortcodeTemplatesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
