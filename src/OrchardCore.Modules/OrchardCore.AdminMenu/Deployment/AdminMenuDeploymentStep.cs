using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment
{
    /// <summary>
    /// Adds all admin menus to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AdminMenuDeploymentStep : DeploymentStep
    {
        public AdminMenuDeploymentStep()
        {
            Name = "AdminMenu";
        }
    }
}
