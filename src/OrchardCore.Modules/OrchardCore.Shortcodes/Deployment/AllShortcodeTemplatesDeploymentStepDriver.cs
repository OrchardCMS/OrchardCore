using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Deployment;

public sealed class AllShortcodeTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllShortcodeTemplatesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllShortcodeTemplatesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllShortcodeTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllShortcodeTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllShortcodeTemplatesDeploymentStep step, BuildEditorContext context)
    {
        return View("AllShortcodeTemplatesDeploymentStep_Edit", step).Location("Content");
    }
}
