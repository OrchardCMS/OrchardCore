using OrchardCore.Deployment;

namespace OrchardCore.Search.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
