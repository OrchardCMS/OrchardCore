using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Themes.TheAdmin.Deployment;

public sealed class AdminSchemesDeploymentStepDriver : DisplayDriver<DeploymentStep, AdminSchemesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AdminSchemesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AdminSchemesDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AdminSchemesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AdminSchemesDeploymentStep step, BuildEditorContext context)
    {
        return View("AdminSchemesDeploymentStep_Fields_Edit", step).Location("Content");
    }
}
