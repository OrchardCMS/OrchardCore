using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllAdminTemplates";

    public static LocalizationSource Category => CommonLocalizationSources.Development;

    public static LocalizationSource Title => TemplatesLocalizationSources.AllAdminTemplates;

    public bool ExportAsFiles { get; set; }
}
