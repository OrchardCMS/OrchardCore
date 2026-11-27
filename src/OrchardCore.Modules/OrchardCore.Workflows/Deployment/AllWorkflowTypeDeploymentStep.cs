using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllWorkflowType";

    public static LocalizationSource Category => WorkflowsLocalizationSources.Workflows;

    public static LocalizationSource Title => WorkflowsLocalizationSources.AllWorkflowTypes;
}
