using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    public AllAdminTemplatesDeploymentStep()
    {
        Name = "AllAdminTemplates";
        Category = LocalizedString.Create("Development");
        Title = LocalizedString.Create("All Admin Templates");
    }
    public bool ExportAsFiles { get; set; }
}
