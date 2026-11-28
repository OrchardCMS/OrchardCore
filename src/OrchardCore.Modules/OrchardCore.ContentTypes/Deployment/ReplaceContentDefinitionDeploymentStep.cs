using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "ReplaceContentDefinition";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => ContentTypesLocalizationSources.ReplaceContentDefinitions;

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
