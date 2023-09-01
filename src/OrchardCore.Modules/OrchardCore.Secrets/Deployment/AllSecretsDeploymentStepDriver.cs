using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Secrets.Deployment;

public class AllSecretsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSecretsDeploymentStep>
{
    public override IDisplayResult Display(AllSecretsDeploymentStep step)
    {
        return
            Combine(
                View("AllSecretsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("AllSecretsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllSecretsDeploymentStep step) =>
        View("AllSecretsDeploymentStep_Fields_Edit", step).Location("Content");
}
