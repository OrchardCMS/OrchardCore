using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Deployment;

public sealed class AllShortcodeTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllShortcodeTemplatesDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllShortcodeTemplatesDeploymentStepDriver(IStringLocalizer<AllShortcodeTemplatesDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllShortcodeTemplatesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllShortcodeTemplatesDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllShortcodeTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllShortcodeTemplatesDeploymentStep step, BuildEditorContext context)
    {
        return View("AllShortcodeTemplatesDeploymentStep_Edit", step).Location(l => l.Zone("Content").Card(S["Shortcode templates"], "10"));
    }
}
