using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep
{
    public override string Name => "AzureAISearchIndexReset";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Search;

    [JsonIgnore]
    public override LocalizationSource Title => AzureAILocalizationSources.ResetAzureAISearchIndices;

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
