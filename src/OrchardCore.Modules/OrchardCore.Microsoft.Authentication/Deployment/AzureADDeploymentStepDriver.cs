using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class AzureADDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureADDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AzureADDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AzureADDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AzureADDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AzureADDeploymentStep step, BuildEditorContext context)
    {
        return View("AzureADDeploymentStep_Edit", step).Location("Content");
    }
}
