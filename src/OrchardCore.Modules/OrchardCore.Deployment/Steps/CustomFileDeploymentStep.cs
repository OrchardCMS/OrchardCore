using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Deployment", typeof(CustomFileDeploymentStep));
    private static readonly LocalizationSource s_title = new("Custom File", typeof(CustomFileDeploymentStep));

    public CustomFileDeploymentStep()
    {
        Name = nameof(CustomFileDeploymentStep);
        Category = s_category;
        Title = s_title;
    }

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
