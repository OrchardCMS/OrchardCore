using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllShortcodesDeploymentStep>
    {
        public override IDisplayResult Display(AllShortcodesDeploymentStep step)
        {
            return
                Combine(
                    View("AllShortcodesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllShortcodesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllShortcodesDeploymentStep step)
        {
            return View("AllShortcodesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
