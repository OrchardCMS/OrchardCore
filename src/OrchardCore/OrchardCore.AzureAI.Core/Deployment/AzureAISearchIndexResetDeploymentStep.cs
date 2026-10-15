using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AzureAISearchIndexResetDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AzureAISearchIndexResetDeploymentStep>("Reset Azure AI Search Indices");

    public AzureAISearchIndexResetDeploymentStep()
    {
        Name = "AzureAISearchIndexReset";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
