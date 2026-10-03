using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Development", typeof(AllTemplatesDeploymentStep));
    private static readonly LocalizationSource s_title = new("All Templates", typeof(AllTemplatesDeploymentStep));

    public AllTemplatesDeploymentStep()
    {
        Name = "AllTemplates";
        Category = s_category;
        Title = s_title;
    }
    public bool ExportAsFiles { get; set; }
}
