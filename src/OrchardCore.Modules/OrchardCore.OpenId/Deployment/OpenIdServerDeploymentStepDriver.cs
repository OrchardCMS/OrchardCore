using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdServerDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdServerDeploymentStep>
    {
        public override IDisplayResult Display(OpenIdServerDeploymentStep step)
        {
            return
                Combine(
                    View("OpenIdServerDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("OpenIdServerDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(OpenIdServerDeploymentStep step)
        {
            return View("OpenIdServerDeploymentStep_Edit", step).Location("Content");
        }
    }
}
