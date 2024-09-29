using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentStepDriver : DisplayDriver<DeploymentStep, MicrosoftAccountDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(MicrosoftAccountDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("MicrosoftAccountDeploymentStep_Summary", step).Location("Summary", "Content"),
            View("MicrosoftAccountDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(MicrosoftAccountDeploymentStep step, BuildEditorContext context)
        => View("MicrosoftAccountDeploymentStep_Edit", step).Location("Content");
}
