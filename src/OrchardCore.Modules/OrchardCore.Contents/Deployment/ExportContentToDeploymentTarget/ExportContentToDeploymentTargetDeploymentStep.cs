using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

/// <summary>
/// Adds content selected with export content to deployment plan target feature to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ExportContentToDeploymentTargetDeploymentStep : DeploymentStep
{
    public override string Name => nameof(ExportContentToDeploymentTargetDeploymentStep);

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => ContentsLocalizationSources.ExportContentToDeploymentTarget;
}
