using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => nameof(CustomFileDeploymentStep);

    public static LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    public static LocalizationSource Title => DeploymentLocalizationSources.CustomFile;

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
