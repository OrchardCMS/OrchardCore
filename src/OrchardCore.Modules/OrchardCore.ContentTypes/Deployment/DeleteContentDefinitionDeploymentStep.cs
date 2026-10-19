using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Deletes selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeleteContentDefinitionDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public DeleteContentDefinitionDeploymentStep()
    {
        Name = "DeleteContentDefinition";
    }

    public string[] ContentTypes { get; set; } = [];

    public string[] ContentParts { get; set; } = [];
}
