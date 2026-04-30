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
    }

    public AllTemplatesDeploymentStep(IStringLocalizer<AllTemplatesDeploymentStep> S)
        : this()
    {
        Category = S["Development"];
    }
    public bool ExportAsFiles { get; set; }
}
