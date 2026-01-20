using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    /// <summary>
    /// Adds layers to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class LuceneIndexDeploymentStep : DeploymentStep
    {
        public LuceneIndexDeploymentStep()
        {
            Name = "LuceneIndex";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] IndexNames { get; set; }
    }
}
