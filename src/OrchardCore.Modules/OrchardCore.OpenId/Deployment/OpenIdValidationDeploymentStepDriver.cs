using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdValidationDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdValidationDeploymentStep>
    {
        public override IDisplayResult Display(OpenIdValidationDeploymentStep step)
        {
            return
                Combine(
                    View("OpenIdValidationDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("OpenIdValidationDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(OpenIdValidationDeploymentStep step)
        {
            return View("OpenIdValidationDeploymentStep_Edit", step).Location("Content");
        }
    }
}
