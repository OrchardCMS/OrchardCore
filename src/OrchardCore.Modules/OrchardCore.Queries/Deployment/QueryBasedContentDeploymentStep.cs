using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

/// <summary>
/// Adds all content items from the result of a Query to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class QueryBasedContentDeploymentStep : DeploymentStep
{
    public override string Name => "QueryBasedContentDeploymentStep";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => QueriesLocalizationSources.QueriedContentItems;

    [Required]
    public string QueryName { get; set; }
    public string QueryParameters { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
