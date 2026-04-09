using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
