using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all content items from the result of a Query to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class QueryBasedContentDeploymentStep : DeploymentStep
{
    public QueryBasedContentDeploymentStep()
    {
        Name = "QueryBasedContentDeploymentStep";
    }

    public QueryBasedContentDeploymentStep(IStringLocalizer<QueryBasedContentDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }

    [Required]
    public string QueryName { get; set; }
    public string QueryParameters { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
