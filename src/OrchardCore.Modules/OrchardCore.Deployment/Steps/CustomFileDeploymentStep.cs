using OrchardCore.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class CustomFileDeploymentStep : DeploymentStep
{
    public override string Name => nameof(CustomFileDeploymentStep);

    [JsonIgnore]
    public override LocalizationSource Category => DeploymentLocalizationSources.Deployment;

    [JsonIgnore]
    public override LocalizationSource Title => DeploymentLocalizationSources.CustomFile;

    [Required]
    public string FileName { get; set; }

    public string FileContent { get; set; }
}
