using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllTemplatesDeploymentStep>("Development");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllTemplatesDeploymentStep>("All Templates");

    public AllTemplatesDeploymentStep()
    {
        Name = "AllTemplates";
        Category = s_category;
        Title = s_title;
    }
    public bool ExportAsFiles { get; set; }
}
