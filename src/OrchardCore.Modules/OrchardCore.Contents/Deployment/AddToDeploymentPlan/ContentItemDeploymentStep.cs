using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

/// <summary>
/// Adds a content item to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentItemDeploymentStep : DeploymentStep
{
    public ContentItemDeploymentStep()
    {
        Name = nameof(ContentItemDeploymentStep);
        Category = LocalizedString.Create("Content Management");
    }

    public string ContentItemId { get; set; }
}
