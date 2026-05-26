using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
    }

    public AllWorkflowTypeDeploymentStep(IStringLocalizer<AllWorkflowTypeDeploymentStep> S)
        : this()
    {
        Category = S["Workflows"];
    }
}
