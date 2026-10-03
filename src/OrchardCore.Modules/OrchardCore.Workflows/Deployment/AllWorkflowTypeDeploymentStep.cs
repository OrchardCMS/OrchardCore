using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Workflows", typeof(AllWorkflowTypeDeploymentStep));
    private static readonly LocalizationSource s_title = new("All Workflow Types", typeof(AllWorkflowTypeDeploymentStep));

    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
        Category = s_category;
        Title = s_title;
    }
}
