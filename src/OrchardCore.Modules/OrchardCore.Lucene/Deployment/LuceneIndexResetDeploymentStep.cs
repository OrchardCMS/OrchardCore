using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment
{
    /// <summary>
    /// Adds reset lucene index task to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class LuceneIndexResetDeploymentStep : DeploymentStep
    {
        public LuceneIndexResetDeploymentStep()
        {
            Name = "LuceneIndexReset";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] IndexNames { get; set; }
    }
}
