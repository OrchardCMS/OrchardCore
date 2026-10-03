using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

/// <summary>
/// Adds a content item to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentItemDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(ContentItemDeploymentStep));

    public ContentItemDeploymentStep()
    {
        Name = nameof(ContentItemDeploymentStep);
        Category = s_category;
    }

    public string ContentItemId { get; set; }
}
