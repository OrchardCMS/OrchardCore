using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
        Category = CommonLocalizationSources.Search;
        Title = AzureAILocalizationSources.AzureAISearchIndexes;
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
