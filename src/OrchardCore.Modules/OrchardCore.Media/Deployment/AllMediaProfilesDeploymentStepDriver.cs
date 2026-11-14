using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Media.Deployment;

public sealed class AllMediaProfilesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllMediaProfilesDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllMediaProfilesDeploymentStepDriver(IStringLocalizer<AllMediaProfilesDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllMediaProfilesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllMediaProfilesDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllMediaProfilesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllMediaProfilesDeploymentStep step, BuildEditorContext context)
    {
        return View("AllMediaProfilesDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Media profiles"], "10"));
    }
}
