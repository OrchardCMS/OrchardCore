using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class MediaDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(MediaDeploymentStep));
    private static readonly LocalizationSource s_title = new("Media", typeof(MediaDeploymentStep));

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
