using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(AzureAISearchIndexResetDeploymentStep));
    private static readonly LocalizationSource s_title = new("Reset Azure AI Search Indices", typeof(AzureAISearchIndexResetDeploymentStep));

    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
