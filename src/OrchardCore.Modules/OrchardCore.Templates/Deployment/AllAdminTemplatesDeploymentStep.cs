using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    public AllAdminTemplatesDeploymentStep()
    {
        Name = "AllAdminTemplates";
        Category = CommonLocalizationSources.Development;
        Title = TemplatesLocalizationSources.AllAdminTemplates;
    }
    public bool ExportAsFiles { get; set; }
}
