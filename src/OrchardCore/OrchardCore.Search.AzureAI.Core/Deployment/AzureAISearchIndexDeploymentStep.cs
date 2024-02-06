using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexDeploymentStep()
    {
        Name = "AzureAISearchIndexSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
