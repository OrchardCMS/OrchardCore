using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    public override string Name => "AllWorkflowType";

    [JsonIgnore]
    public override LocalizationSource Category => WorkflowsLocalizationSources.Workflows;

    [JsonIgnore]
    public override LocalizationSource Title => WorkflowsLocalizationSources.AllWorkflowTypes;
}
