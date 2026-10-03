using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

/// <summary>
/// Adds content selected with export content to deployment plan target feature to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ExportContentToDeploymentTargetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(ExportContentToDeploymentTargetDeploymentStep));
    private static readonly LocalizationSource s_title = new("Export Content To Deployment Target", typeof(ExportContentToDeploymentTargetDeploymentStep));

    public ExportContentToDeploymentTargetDeploymentStep()
    {
        Name = nameof(ExportContentToDeploymentTargetDeploymentStep);
        Category = s_category;
        Title = s_title;
    }
}
