using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllContentDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllContent";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => ContentsLocalizationSources.AllContent;

    public bool ExportAsSetupRecipe { get; set; }
}
