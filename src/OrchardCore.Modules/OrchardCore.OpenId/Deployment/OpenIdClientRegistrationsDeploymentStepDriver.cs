using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment;

public sealed class OpenIdClientRegistrationsDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdClientRegistrationsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(OpenIdClientRegistrationsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("OpenIdClientRegistrationsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("OpenIdClientRegistrationsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(OpenIdClientRegistrationsDeploymentStep step, BuildEditorContext context)
    {
        return View("OpenIdClientRegistrationsDeploymentStep_Edit", step).Location("Content");
    }
}
