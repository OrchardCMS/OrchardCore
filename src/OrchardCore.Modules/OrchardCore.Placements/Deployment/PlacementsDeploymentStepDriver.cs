using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Placements.Deployment;

public sealed class PlacementsDeploymentStepDriver : DisplayDriver<DeploymentStep, PlacementsDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public PlacementsDeploymentStepDriver(IStringLocalizer<PlacementsDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(PlacementsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("PlacementsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("PlacementsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(PlacementsDeploymentStep step, BuildEditorContext context)
    {
        return View("PlacementsDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Placements"], "10"));
    }
}
