using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Placements.Deployment;

public sealed class PlacementsDeploymentStepDriver : DisplayDriver<DeploymentStep, PlacementsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(PlacementsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("PlacementsDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("PlacementsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(PlacementsDeploymentStep step, BuildEditorContext context)
    {
        return View("PlacementsDeploymentStep_Edit", step).Location("Content");
    }
}
