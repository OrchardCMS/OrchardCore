using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Workflows;

    [JsonIgnore]
    public override LocalizationSource Title => WorkflowsLocalizationSources.AllWorkflowTypes;

    public AllWorkflowTypeDeploymentStep()
    {
        Name = "AllWorkflowType";
    }
}
