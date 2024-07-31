using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Deployment;

public class AllUsersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllUsersDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllUsersDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
                View("AllUsersDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllUsersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override Task<IDisplayResult> EditAsync(AllUsersDeploymentStep step, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("AllUsersDeploymentStep_Edit", step).Location("Content")
        );
    }
}
