using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all content items from the result of a Query to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class QueryBasedContentDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "QueryBasedContentDeploymentStep";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => QueriesLocalizationSources.QueriedContentItems;

    [Required]
    public string QueryName { get; set; }
    public string QueryParameters { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
