using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

/// <summary>
/// Adds users to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllUsersDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Security", typeof(AllUsersDeploymentStep));

    public AllUsersDeploymentStep()
    {
        Name = "AllUsers";
        Category = s_category;
    }
}
