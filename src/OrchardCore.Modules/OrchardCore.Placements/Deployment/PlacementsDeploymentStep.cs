using OrchardCore.Deployment;

namespace OrchardCore.Placements.Deployment
{
    /// <summary>
    /// Adds placements to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class PlacementsDeploymentStep : DeploymentStep
    {
        public PlacementsDeploymentStep()
        {
            Name = "Placements";
        }
    }
}
