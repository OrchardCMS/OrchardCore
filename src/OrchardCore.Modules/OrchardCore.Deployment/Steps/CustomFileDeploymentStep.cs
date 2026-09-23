using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    public CustomFileDeploymentStep()
    {
        Name = nameof(CustomFileDeploymentStep);
        Category = LocalizedString.Create("Deployment");
    }

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
