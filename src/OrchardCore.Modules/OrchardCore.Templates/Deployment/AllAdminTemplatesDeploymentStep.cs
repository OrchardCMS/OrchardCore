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
    }

    public AllAdminTemplatesDeploymentStep(IStringLocalizer<AllAdminTemplatesDeploymentStep> S)
        : this()
    {
        Category = S["Development"];
    }
    public bool ExportAsFiles { get; set; }
}
