using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AzureAISearchIndexRebuildDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AzureAISearchIndexRebuildDeploymentStep>("Rebuild Azure AI Search Indices");

    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
