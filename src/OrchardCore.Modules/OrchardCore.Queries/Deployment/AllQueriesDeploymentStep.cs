using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment
{
    /// <summary>
    /// Adds all queries to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllQueriesDeploymentStep : DeploymentStep
    {
        public AllQueriesDeploymentStep()
        {
            Name = "AllQueries";
        }
    }
}
