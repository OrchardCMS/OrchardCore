using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Secrets.Deployment;

public class AllSecretsRsaDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSecretsRsaDeploymentStep>
{
    public override IDisplayResult Display(AllSecretsRsaDeploymentStep step)
    {
        return
            Combine(
                View("AllSecretsRsaDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("AllSecretsRsaDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllSecretsRsaDeploymentStep step) =>
        View("AllSecretsRsaDeploymentStep_Fields_Edit", step).Location("Content");
}
