using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexRebuildDeploymentStep()
    {
        Name = "AzureAISearchIndexRebuild";
        Category = CommonLocalizationSources.Search;
        Title = AzureAILocalizationSources.RebuildAzureAISearchIndices;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
