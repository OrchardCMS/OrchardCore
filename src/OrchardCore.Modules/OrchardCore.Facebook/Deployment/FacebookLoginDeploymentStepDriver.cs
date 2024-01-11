using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Facebook.Deployment
{
    public class FacebookLoginDeploymentStepDriver : DisplayDriver<DeploymentStep, FacebookLoginDeploymentStep>
    {
        public override IDisplayResult Display(FacebookLoginDeploymentStep step)
        {
            return
                Combine(
                    View("FacebookLoginDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("FacebookLoginDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(FacebookLoginDeploymentStep step)
        {
            return View("FacebookLoginDeploymentStep_Edit", step).Location("Content");
        }
    }
}
