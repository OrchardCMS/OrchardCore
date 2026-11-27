using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

/// <summary>
/// Adds content selected with export content to deployment plan target feature to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ExportContentToDeploymentTargetDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => nameof(ExportContentToDeploymentTargetDeploymentStep);

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => ContentsLocalizationSources.ExportContentToDeploymentTarget;
}
