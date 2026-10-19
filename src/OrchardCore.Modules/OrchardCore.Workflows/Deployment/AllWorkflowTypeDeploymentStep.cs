using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
        Category = CommonLocalizationSources.Workflows;
        Title = WorkflowsLocalizationSources.AllWorkflowTypes;
    }
}
