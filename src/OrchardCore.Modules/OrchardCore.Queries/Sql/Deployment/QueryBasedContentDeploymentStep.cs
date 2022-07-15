using System.ComponentModel.DataAnnotations;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Sql.Deployment
{
    /// <summary>
    /// Adds all content items based on SQL query results to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class QueryBasedContentDeploymentStep : DeploymentStep
    {
        public QueryBasedContentDeploymentStep()
        {
            Name = "QueryBasedContentDeploymentStep";
        }

        [Required]
        public string QueryName { get; set; }
        public string QueryParameters { get; set; }
        public bool ExportAsSetupRecipe { get; set; }
    }
}
