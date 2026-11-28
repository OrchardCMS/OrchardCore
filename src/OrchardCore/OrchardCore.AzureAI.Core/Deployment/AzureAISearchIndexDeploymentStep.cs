using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => AzureAISearchIndexSettingsStep.Name;

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => AzureAILocalizationSources.AzureAISearchIndexes;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
