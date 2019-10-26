using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdDeploymentStep>
    {
        public override IDisplayResult Display(OpenIdDeploymentStep step)
        {
            return
                Combine(
                    View("OpenIdDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("OpenIdDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(OpenIdDeploymentStep step)
        {
            return View("OpenIdDeploymentStep_Edit", step).Location("Content");
        }
    }
}
