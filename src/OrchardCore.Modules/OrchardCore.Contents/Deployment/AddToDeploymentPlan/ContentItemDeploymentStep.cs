using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

/// <summary>
/// Adds a content item to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentItemDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => nameof(ContentItemDeploymentStep);

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public string ContentItemId { get; set; }
}
