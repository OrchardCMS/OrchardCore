using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AzureAISearchIndexDeploymentStep>("Search");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AzureAISearchIndexDeploymentStep>("Azure AI Search Indexes");

    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
