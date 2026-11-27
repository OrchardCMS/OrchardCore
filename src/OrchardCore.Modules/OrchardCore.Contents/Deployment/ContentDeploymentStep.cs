using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "ContentDeploymentStep";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => ContentsLocalizationSources.ContentTypes;

    public string[] ContentTypes { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
