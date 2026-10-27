using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDeploymentStep : DeploymentStep
{
    public override string Name => "ContentDeploymentStep";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => ContentsLocalizationSources.ContentTypes;

    public string[] ContentTypes { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
