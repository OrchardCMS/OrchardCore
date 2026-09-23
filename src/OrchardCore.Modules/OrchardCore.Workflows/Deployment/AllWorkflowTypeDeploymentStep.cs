using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
        Category = LocalizedString.Create("Workflows");
    }
}
