using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentStep : DeploymentStep
{
    public override string Name => "ReplaceContentDefinition";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => ContentTypesLocalizationSources.ReplaceContentDefinitions;

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
