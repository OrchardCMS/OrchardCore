using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    /// <summary>
    /// Adds reset Lucene index task to a <see cref="DeploymentPlanResult"/>.
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
