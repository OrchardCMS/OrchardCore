using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(AzureAISearchIndexRebuildDeploymentStep));
    private static readonly LocalizationSource s_title = new("Rebuild Azure AI Search Indices", typeof(AzureAISearchIndexRebuildDeploymentStep));

    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
