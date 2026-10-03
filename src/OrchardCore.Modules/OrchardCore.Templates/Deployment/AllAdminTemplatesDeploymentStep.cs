using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Development", typeof(AllAdminTemplatesDeploymentStep));
    private static readonly LocalizationSource s_title = new("All Admin Templates", typeof(AllAdminTemplatesDeploymentStep));

    public AllAdminTemplatesDeploymentStep()
    {
        Name = "AllAdminTemplates";
        Category = s_category;
        Title = s_title;
    }
    public bool ExportAsFiles { get; set; }
}
