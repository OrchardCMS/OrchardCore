using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllTemplates";

    public static LocalizationSource Category => CommonLocalizationSources.Development;

    public static LocalizationSource Title => TemplatesLocalizationSources.AllTemplates;

    public bool ExportAsFiles { get; set; }
}
