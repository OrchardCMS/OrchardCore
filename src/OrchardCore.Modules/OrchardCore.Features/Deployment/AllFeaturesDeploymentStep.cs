using OrchardCore.Deployment;

namespace OrchardCore.Features.Deployment
{
    /// <summary>
    /// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllFeaturesDeploymentStep : DeploymentStep
    {
        public AllFeaturesDeploymentStep()
        {
            Name = "AllFeatures";
        }

        public bool IgnoreDisabledFeatures { get; set; }
    }
}
