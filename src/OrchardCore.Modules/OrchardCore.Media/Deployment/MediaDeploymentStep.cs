using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class MediaDeploymentStep : DeploymentStep
{
    public override string Name => "Media";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => MediaLocalizationSources.Media;

    public bool IncludeAll { get; set; } = true;

    public string[] FilePaths { get; set; }

    public string[] DirectoryPaths { get; set; }
}
