using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class MediaDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<MediaDeploymentStep>("Content Management");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<MediaDeploymentStep>("Media");

    public MediaDeploymentStep()
    {
        Name = "Media";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] FilePaths { get; set; }

    public string[] DirectoryPaths { get; set; }
}
