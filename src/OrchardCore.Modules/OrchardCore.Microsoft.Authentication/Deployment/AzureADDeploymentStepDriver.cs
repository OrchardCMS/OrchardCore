using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Microsoft.Authentication.Deployment
{
    public class AzureADDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureADDeploymentStep>
    {
        public override IDisplayResult Display(AzureADDeploymentStep step)
        {
            return
                Combine(
                    View("AzureADDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AzureADDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AzureADDeploymentStep step)
        {
            return View("AzureADDeploymentStep_Edit", step).Location("Content");
        }
    }
}
