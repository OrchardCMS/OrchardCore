using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep
{
    public override string Name => "AzureAISearchIndexRebuild";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => AzureAILocalizationSources.RebuildAzureAISearchIndices;

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
