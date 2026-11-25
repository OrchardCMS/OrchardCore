using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminMenu.Deployment;

public sealed class AdminMenuDeploymentStepDriver : DisplayDriver<DeploymentStep, AdminMenuDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AdminMenuDeploymentStepDriver(IStringLocalizer<AdminMenuDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AdminMenuDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AdminMenuDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AdminMenuDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AdminMenuDeploymentStep step, BuildEditorContext context)
    {
        return View("AdminMenuDeploymentStep_Fields_Edit", step).Location(l => l.Zone("Content").Card(S["Admin menus"], "10"));
    }
}
