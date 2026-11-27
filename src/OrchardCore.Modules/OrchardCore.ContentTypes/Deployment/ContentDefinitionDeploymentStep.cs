using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Adds selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDefinitionDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "ContentDefinition";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => ContentTypesLocalizationSources.UpdateContentDefinitions;

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
