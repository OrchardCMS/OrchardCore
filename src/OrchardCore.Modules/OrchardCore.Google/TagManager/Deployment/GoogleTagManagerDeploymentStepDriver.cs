using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Google.TagManager.Deployment
{
    public class GoogleTagManagerDeploymentStepDriver : DisplayDriver<DeploymentStep, GoogleTagManagerDeploymentStep>
    {
        public override IDisplayResult Display(GoogleTagManagerDeploymentStep step)
        {
            return
                Combine(
                    View("GoogleTagManagerDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("GoogleTagManagerDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(GoogleTagManagerDeploymentStep step)
        {
            return View("GoogleTagManagerDeploymentStep_Edit", step).Location("Content");
        }
    }
}
