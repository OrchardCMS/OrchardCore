using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllTemplatesDeploymentStep : DeploymentStep
{
    public AllTemplatesDeploymentStep()
    {
        Name = "AllTemplates";
        Category = LocalizedString.Create("Development");
        Title = LocalizedString.Create("All Templates");
    }
    public bool ExportAsFiles { get; set; }
}
