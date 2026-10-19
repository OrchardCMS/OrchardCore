using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    public CustomFileDeploymentStep()
    {
        Name = nameof(CustomFileDeploymentStep);
        Category = CommonLocalizationSources.Deployment;
        Title = DeploymentLocalizationSources.CustomFile;
    }

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
