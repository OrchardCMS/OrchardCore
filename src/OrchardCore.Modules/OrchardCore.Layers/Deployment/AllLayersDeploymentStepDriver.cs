using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.Deployment;

public sealed class AllLayersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllLayersDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllLayersDeploymentStepDriver(IStringLocalizer<AllLayersDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllLayersDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllLayersDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllLayersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllLayersDeploymentStep step, BuildEditorContext context)
    {
        return View("AllLayersDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Layers"], "10"));
    }
}
