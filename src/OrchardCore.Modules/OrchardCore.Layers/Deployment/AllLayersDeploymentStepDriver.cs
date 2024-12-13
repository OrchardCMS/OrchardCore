using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.Deployment;

public sealed class AllLayersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllLayersDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllLayersDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllLayersDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllLayersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllLayersDeploymentStep step, BuildEditorContext context)
    {
        return View("AllLayersDeploymentStep_Edit", step).Location("Content");
    }
}
