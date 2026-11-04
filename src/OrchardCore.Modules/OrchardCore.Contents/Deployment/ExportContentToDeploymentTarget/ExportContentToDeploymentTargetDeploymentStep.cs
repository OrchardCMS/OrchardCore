using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

/// <summary>
/// Adds content selected with export content to deployment plan target feature to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ExportContentToDeploymentTargetDeploymentStep : DeploymentStep
{
    /// <summary>Gets or sets explicit tenant content IDs; null retains the existing admin form selection.</summary>
    public string[] ContentItemIds { get; set; }

    /// <summary>Gets or sets whether explicit selections export latest instead of published versions.</summary>
    public bool Latest { get; set; }

    public override string Name => nameof(ExportContentToDeploymentTargetDeploymentStep);

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => ContentsLocalizationSources.ExportContentToDeploymentTarget;
}
