using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class MediaDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "Media";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => MediaLocalizationSources.Media;

    public bool IncludeAll { get; set; } = true;

    public string[] FilePaths { get; set; }

    public string[] DirectoryPaths { get; set; }
}
