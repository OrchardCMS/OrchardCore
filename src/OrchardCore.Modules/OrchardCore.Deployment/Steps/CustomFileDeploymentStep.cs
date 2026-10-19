using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.CustomFile;

    public CustomFileDeploymentStep()
    {
        Name = nameof(CustomFileDeploymentStep);
    }

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
