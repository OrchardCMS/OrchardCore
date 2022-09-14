using OrchardCore.Deployment;

namespace OrchardCore.Search.Lucene.Deployment
{
    /// <summary>
    /// Adds rebuild Lucene index task to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class LuceneIndexRebuildDeploymentStep : DeploymentStep
    {
        public LuceneIndexRebuildDeploymentStep()
        {
            Name = "LuceneIndexRebuild";
        }

        public bool IncludeAll { get; set; } = true;

        public string[] IndexNames { get; set; }
    }
}
