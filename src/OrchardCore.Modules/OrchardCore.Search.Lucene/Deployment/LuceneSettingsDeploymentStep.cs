using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    /// <summary>
    /// Adds layers to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class LuceneSettingsDeploymentStep : DeploymentStep
    {
        public LuceneSettingsDeploymentStep()
        {
            Name = "LuceneSettings";
        }
    }
}
