using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Secrets.Deployment;

public sealed class SecretsDeploymentStepDriver : DisplayDriver<DeploymentStep, SecretsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(SecretsDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("SecretsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("SecretsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(SecretsDeploymentStep step, BuildEditorContext context)
    {
        return View("SecretsDeploymentStep_Edit", step).Location("Content");
    }
}
