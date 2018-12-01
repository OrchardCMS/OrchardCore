using OrchardCore.Deployment;

namespace OrchardCore.AdminTrees.Deployment
{
    /// <summary>
    /// Adds all admin trees to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class AdminTreesDeploymentStep : DeploymentStep
    {
        public AdminTreesDeploymentStep()
        {
            Name = "AdminTrees";
        }
    }
}
