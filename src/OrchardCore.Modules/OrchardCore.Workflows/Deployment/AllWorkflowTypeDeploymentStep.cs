using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllWorkflowTypeDeploymentStep>("Workflows");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllWorkflowTypeDeploymentStep>("All Workflow Types");

    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
        Category = s_category;
        Title = s_title;
    }
}
