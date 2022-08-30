using OrchardCore.Deployment;

namespace OrchardCore.Lucene.Deployment
{
    /// <summary>
    /// Adds rebuild lucene index task to a <see cref="DeploymentPlanResult"/>.
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
