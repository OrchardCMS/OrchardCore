using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllDataTranslationsDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllDataTranslationsDeploymentStepDriver(IStringLocalizer<AllDataTranslationsDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllDataTranslationsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllDataTranslationsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllDataTranslationsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllDataTranslationsDeploymentStep step, BuildEditorContext context)
    {
        return View("AllDataTranslationsDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Dynamic data translations"], "10"));
    }
}
