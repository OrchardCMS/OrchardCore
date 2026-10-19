using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

/// <summary>
/// Adds a content item to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentItemDeploymentStep : DeploymentStep
{
    public override string Name => nameof(ContentItemDeploymentStep);

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public string ContentItemId { get; set; }
}
