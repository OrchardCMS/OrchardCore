using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment;

public sealed class OpenIdServerDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdServerDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(OpenIdServerDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("OpenIdServerDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("OpenIdServerDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(OpenIdServerDeploymentStep step, BuildEditorContext context)
    {
        return View("OpenIdServerDeploymentStep_Edit", step).Location("Content");
    }
}
