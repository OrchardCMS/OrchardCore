using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Deletes selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeleteContentDefinitionDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "DeleteContentDefinition";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public string[] ContentTypes { get; set; } = [];

    public string[] ContentParts { get; set; } = [];
}
