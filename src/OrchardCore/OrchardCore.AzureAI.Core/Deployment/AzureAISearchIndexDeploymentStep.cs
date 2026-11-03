using System.Text.Json.Serialization;
using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public override string Name => AzureAISearchIndexSettingsStep.Name;

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => AzureAILocalizationSources.AzureAISearchIndexes;

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
