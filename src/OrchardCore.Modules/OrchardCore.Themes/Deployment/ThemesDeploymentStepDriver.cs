using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Themes.Deployment;

public sealed class ThemesDeploymentStepDriver : DisplayDriver<DeploymentStep, ThemesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(ThemesDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
                View("ThemesDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("ThemesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ThemesDeploymentStep step, BuildEditorContext context)
    {
        return View("ThemesDeploymentStep_Edit", step).Location("Content");
    }
}
