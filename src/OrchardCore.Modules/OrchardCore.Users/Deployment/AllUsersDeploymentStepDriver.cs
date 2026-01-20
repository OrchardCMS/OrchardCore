using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Deployment;

public class AllUsersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllUsersDeploymentStep>
{
    public override IDisplayResult Display(AllUsersDeploymentStep step)
    {
        return Combine(
                View("AllUsersDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllUsersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override IDisplayResult Edit(AllUsersDeploymentStep step)
    {
        return View("AllUsersDeploymentStep_Edit", step).Location("Content");
    }
}
