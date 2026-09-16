using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

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

    public ExportContentToDeploymentTargetDeploymentStep()
    {
        Name = nameof(ExportContentToDeploymentTargetDeploymentStep);
    }

    public ExportContentToDeploymentTargetDeploymentStep(IStringLocalizer<ExportContentToDeploymentTargetDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }
}
