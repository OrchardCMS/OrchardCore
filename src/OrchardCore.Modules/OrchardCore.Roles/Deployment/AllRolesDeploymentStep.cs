using OrchardCore.Deployment;

namespace OrchardCore.Roles.Deployment
{
    /// <summary>
    /// Adds roles to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllRolesDeploymentStep : DeploymentStep
    {
        public AllRolesDeploymentStep()
        {
            Name = "AllRoles";
        }
    }
}
