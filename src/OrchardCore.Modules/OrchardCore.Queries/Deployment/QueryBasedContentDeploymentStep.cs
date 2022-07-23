using System.ComponentModel.DataAnnotations;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment
{
    /// <summary>
    /// Adds all content items from the result of a Query to a <see cref="DeploymentPlanResult"/>.
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
