using OrchardCore.Deployment;

namespace OrchardCore.Users.Deployment;

/// <summary>
/// Adds users to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllUsersDeploymentStep : DeploymentStep
{
    public AllUsersDeploymentStep()
    {
        Name = "AllUsers";
    }
}
