using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tenants.Deployment;

public sealed class AllFeatureProfilesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllFeatureProfilesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllFeatureProfilesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllFeatureProfilesDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllFeatureProfilesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllFeatureProfilesDeploymentStep step, BuildEditorContext context)
    {
        return View("AllFeatureProfilesDeploymentStep_Edit", step).Location("Content");
    }
}
