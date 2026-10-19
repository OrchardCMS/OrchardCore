using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep
{
    public AllTemplatesDeploymentStep()
    {
        Name = "AllTemplates";
        Category = CommonLocalizationSources.Development;
        Title = TemplatesLocalizationSources.AllTemplates;
    }
    public bool ExportAsFiles { get; set; }
}
