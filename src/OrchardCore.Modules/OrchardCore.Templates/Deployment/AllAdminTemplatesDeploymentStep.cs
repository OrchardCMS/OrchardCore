using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllAdminTemplatesDeploymentStep>("Development");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllAdminTemplatesDeploymentStep>("All Admin Templates");

    public AllAdminTemplatesDeploymentStep()
    {
        Name = "AllAdminTemplates";
        Category = s_category;
        Title = s_title;
    }
    public bool ExportAsFiles { get; set; }
}
