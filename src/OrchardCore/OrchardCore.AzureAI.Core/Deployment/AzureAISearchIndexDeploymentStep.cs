using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Search", typeof(AzureAISearchIndexDeploymentStep));
    private static readonly LocalizationSource s_title = new("Azure AI Search Indexes", typeof(AzureAISearchIndexDeploymentStep));

    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
