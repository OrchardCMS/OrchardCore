using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Roles.Deployment;

public sealed class AllRolesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllRolesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllRolesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllRolesDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllRolesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllRolesDeploymentStep step, BuildEditorContext context)
    {
        return View("AllRolesDeploymentStep_Edit", step).Location("Content");
    }
}
