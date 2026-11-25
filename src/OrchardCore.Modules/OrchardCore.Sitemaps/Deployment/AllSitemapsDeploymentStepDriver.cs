using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSitemapsDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllSitemapsDeploymentStepDriver(IStringLocalizer<AllSitemapsDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllSitemapsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllSitemapsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllSitemapsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllSitemapsDeploymentStep step, BuildEditorContext context)
    {
        return View("AllSitemapsDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Sitemaps"], "10"));
    }
}
