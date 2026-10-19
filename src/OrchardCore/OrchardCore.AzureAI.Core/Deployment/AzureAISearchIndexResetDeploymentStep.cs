using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
        Category = CommonLocalizationSources.Search;
        Title = AzureAILocalizationSources.ResetAzureAISearchIndices;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
