using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    public override string Name => "AllWorkflowType";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Workflows;

    [JsonIgnore]
    public override LocalizationSource Title => WorkflowsLocalizationSources.AllWorkflowTypes;
}
