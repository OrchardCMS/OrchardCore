using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Themes.Schemes.Deployment;

internal sealed class ThemeSchemesDeploymentStepDriver : DisplayDriver<DeploymentStep, ThemeSchemesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(ThemeSchemesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ThemeSchemesDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("ThemeSchemesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ThemeSchemesDeploymentStep step, BuildEditorContext context)
    {
        return View("ThemeSchemesDeploymentStep_Fields_Edit", step).Location("Content");
    }
}
