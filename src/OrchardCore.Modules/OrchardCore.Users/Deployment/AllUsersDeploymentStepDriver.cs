using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Deployment;

public sealed class AllUsersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllUsersDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllUsersDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
                View("AllUsersDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllUsersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override IDisplayResult Edit(AllUsersDeploymentStep step, BuildEditorContext context)
    {
        return View("AllUsersDeploymentStep_Edit", step).Location("Content");
    }
}
