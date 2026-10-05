using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<CustomFileDeploymentStep>("Deployment");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<CustomFileDeploymentStep>("Custom File");

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
