using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all content items from the result of a Query to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class QueryBasedContentDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<QueryBasedContentDeploymentStep>("Content Management");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<QueryBasedContentDeploymentStep>("Queried Content Items");

    public QueryBasedContentDeploymentStep()
    {
        Name = "QueryBasedContentDeploymentStep";
        Category = s_category;
        Title = s_title;
    }

    [Required]
    public string QueryName { get; set; }
    public string QueryParameters { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
